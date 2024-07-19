using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Progetto.App.Core.Data;
using Progetto.App.Core.Models;
using Progetto.App.Core.Models.Mqtt;
using Progetto.App.Core.Repositories;
using System.Text;
using System.Text.Json;

namespace Progetto.App.Core.Services.MQTT;

/// <summary>
/// MwBot MQTT client (curresponds to single instance of mwbot when it's online)
/// </summary>
public class MqttMwBotClient
{
    private MqttClientOptions? _options;
    private readonly IMqttClient _mqttClient;
    private readonly ILogger<MqttMwBotClient> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ChargeManager _chargeManager;
    private CurrentlyChargingRepository _currentlyChargingRepository;
    private MwBotRepository _mwBotRepository;
    private Timer _timer;
    public MwBot? mwBot { get; private set; }
    public ImmediateRequest? HandlingRequest { get; private set; }
    public CurrentlyCharging? CurrentlyCharging { get; private set; }

    public MqttMwBotClient(ILogger<MqttMwBotClient> logger, IServiceScopeFactory serviceScopeFactory, ChargeManager chargeManager)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _chargeManager = chargeManager;

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _currentlyChargingRepository = new CurrentlyChargingRepository(dbContext);
            _mwBotRepository = new MwBotRepository(dbContext);
        }

        _mqttClient = new MqttFactory().CreateMqttClient();
        _mqttClient.ApplicationMessageReceivedAsync += HandleReceivedApplicationMessage;

        // Set timer for processing charge requests
        _timer = new Timer(TimedProcessChargeRequest, null, Timeout.Infinite, 10 * 1000);
    }

    /// <summary>
    /// Initialize MwBot client with given id and connect to MQTT server
    /// </summary>
    /// <param name="mwBotId"></param>
    /// <param name="brokerAddress"></param>
    /// <returns>Connection success/failure</returns>
    public async Task<bool> InitializeAsync(int? mwBotId, string brokerAddress = "localhost")
    {
        _options = new MqttClientOptionsBuilder()
            .WithClientId($"mwbot{mwBotId}")
            .WithTcpServer(brokerAddress, 1883)
            //.WithTimeout(TimeSpan.FromSeconds(10))
            //.WithKeepAlivePeriod(TimeSpan.FromSeconds(60))
            .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithCleanSession()
            .Build();

        await InitializeMwBot(mwBotId);
        var isConnected = await ConnectAsync();
        if (isConnected)
        {
            _timer.Change(0, 10000); // Initialize timer at 10 seconds
        }
        return isConnected;
    }

    /// <summary>
    /// Initialize MwBot with given id if exists in database
    /// </summary>
    /// <param name="mwBotId"></param>
    /// <returns></returns>
    private async Task InitializeMwBot(int? mwBotId)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mwBotRepository = scope.ServiceProvider.GetRequiredService<MwBotRepository>();

            if (mwBotId is not null)
                mwBot = await mwBotRepository.GetByIdAsync(mwBotId.Value);

            if (mwBot is null)
            {
                _logger.LogWarning("MwBot not found");
                return;
            }

            _logger.LogInformation("MwBot {mwBotId} initialized successfully", mwBotId);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while initializing MwBot with id: {mwBotId}", mwBotId);
        }
    }

    public async Task PublishAsync(string payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var message = new MqttApplicationMessageBuilder()
            .WithPayload(payload)
            .WithTopic($"mwbot{mwBot?.Id}")
            .Build();

        if (!_mqttClient.IsConnected)
        {
            _logger.LogWarning("Client is not connected, cannot publish message");
            return;
        }

        await _mqttClient.PublishAsync(message);
        _logger.LogInformation("Published message {payload}", payload);
    }

    private async Task<bool> ChangeBotStatus(MwBotStatus status)
    {
        if (mwBot is null)
        {
            _logger.LogWarning("MwBot not initialized, cannot change status");
            return false;
        }

        switch (status)
        {
            case MwBotStatus.Offline:
                _logger.LogInformation("Disconnecting from MQTT server");
                await DisconnectAsync();
                break;
            case MwBotStatus.StandBy:
                _logger.LogInformation("Awaiting for task");
                break;
            case MwBotStatus.ChargingCar:
                _logger.LogInformation("Charging car");
                break;
            case MwBotStatus.Recharging:
                _logger.LogInformation("Recharging");
                break;
            case MwBotStatus.MovingToSlot:
                _logger.LogInformation("Going to charge car on parking slot");
                break;
            case MwBotStatus.MovingToDock:
                _logger.LogInformation("Going to dock for recharge");
                break;
            default:
                _logger.LogWarning("Invalid status");
                return false;
        }

        mwBot.Status = status;
        using var scope = _serviceScopeFactory.CreateScope();
        var mwBotRepository = scope.ServiceProvider.GetRequiredService<MwBotRepository>();
        await mwBotRepository.UpdateAsync(mwBot);

        return true;
    }

    /// <summary>
    /// EVENT: What to do every _timer interval
    /// </summary>
    /// <param name="state"></param>
    private async void TimedProcessChargeRequest(object? state)
    {
        if (mwBot?.Status == MwBotStatus.StandBy)
        {
            HandlingRequest = await _chargeManager.ServeNext(mwBot);

            if (HandlingRequest is not null)
            {
                _logger.BeginScope("Processing request {request}", HandlingRequest);

                // Simulate mwbot movement
                var changeSuccess = await ChangeBotStatus(MwBotStatus.MovingToSlot);
                if (!changeSuccess)
                    return;

                int minDelaySeconds = 10; // 10 minimum seconds
                int maxDelaySeconds = 30; // 30 max seconds
                int delayRange = maxDelaySeconds - minDelaySeconds;
                int delay = minDelaySeconds + (int)(delayRange * (1 - mwBot.BatteryPercentage / 100));
                _logger.LogInformation("Simulating movement for {delay} seconds...", delay);
                await Task.Delay(TimeSpan.FromSeconds(delay));

                await ProcessChargeRequest();
            }
        }
    }

    /// <summary>
    /// Send request to broker to start charging process
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task ProcessChargeRequest()
    {
        if (mwBot is null)
        {
            _logger.LogWarning("MwBot not initialized, cannot process request");
            return;
        }

        var mwBotMessage = new MqttClientMessage
        {
            Id = mwBot.Id,
            Status = mwBot.Status,
            BatteryPercentage = mwBot.BatteryPercentage,
            ParkingSlotId = HandlingRequest.ParkingSlotId,
            TargetBatteryPercentage = HandlingRequest.RequestedChargeLevel,
            UserId = HandlingRequest.UserId,
        };

        await PublishClientMessageAsync(mwBotMessage);
    }

    private async Task SimulateChargingProcess(MqttClientMessage mwBotMessage)
    {
        decimal chargeRate = 0.5m; // kW/s
        decimal dischargeRate = 0.1m; // kW/s
        decimal energyCostPerKw = 0.2m; // Esempio di costo energetico per kW

        using var scope = _serviceScopeFactory.CreateScope();
        var currentlyChargingRepository = scope.ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();

        var changeSuccess = await ChangeBotStatus(MwBotStatus.ChargingCar);
        if (!changeSuccess)
            return;

        while (mwBotMessage.CurrentCarCharge < mwBotMessage.TargetBatteryPercentage && mwBot.BatteryPercentage > 0)
        {
            await Task.Delay(1000); // Simulate one second

            mwBotMessage.CurrentCarCharge += chargeRate;
            mwBot.BatteryPercentage -= dischargeRate;

            if (mwBotMessage.CurrentCarCharge > mwBotMessage.TargetBatteryPercentage)
            {
                mwBotMessage.CurrentCarCharge = mwBotMessage.TargetBatteryPercentage;
            }

            if (mwBot.BatteryPercentage < 0)
            {
                mwBot.BatteryPercentage = 0;
            }

            CurrentlyCharging.EnergyConsumed += chargeRate / 3600; // Convert kW/s to kWh
            CurrentlyCharging.TotalCost = CurrentlyCharging.EnergyConsumed * energyCostPerKw;

            _logger.LogInformation("MwBot {botId}: Charging car: {carCharge}% | MwBot battery: {botBattery}% | Energy consumed: {energyConsumed} kWh | Total cost: {totalCost} EUR",
                mwBot.Id, mwBotMessage.CurrentCarCharge, mwBot.BatteryPercentage, CurrentlyCharging.EnergyConsumed, CurrentlyCharging.TotalCost);

            var mwBotRepository = scope.ServiceProvider.GetRequiredService<MwBotRepository>();
            await mwBotRepository.UpdateAsync(mwBot);
            await currentlyChargingRepository.UpdateAsync(CurrentlyCharging);

            await PublishClientMessageAsync(mwBotMessage);
        }

        await CompleteChargingProcess(mwBotMessage);
    }

    private async Task CompleteChargingProcess(MqttClientMessage mwBotMessage)
    {
        mwBotMessage.Status = MwBotStatus.StandBy;
        await PublishClientMessageAsync(mwBotMessage);
        await ChangeBotStatus(MwBotStatus.StandBy);

        mwBotMessage = new MqttClientMessage
        {
            Id = mwBot.Id,
            Status = mwBot.Status,
            BatteryPercentage = mwBot.BatteryPercentage,
            ParkingSlotId = HandlingRequest.ParkingSlotId,
            TargetBatteryPercentage = HandlingRequest.RequestedChargeLevel
        };

        await PublishClientMessageAsync(mwBotMessage);

        _logger.LogInformation("MwBot {botId}: Completed charging car at parking slot {slotId}. Total energy consumed: {energyConsumed} kWh, Total cost: {totalCost} EUR",
            mwBot?.Id, mwBotMessage.ParkingSlotId, CurrentlyCharging.EnergyConsumed, CurrentlyCharging.TotalCost);
    }

    /// <summary>
    /// Handle received application message from MQTT server
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task HandleReceivedApplicationMessage(MqttApplicationMessageReceivedEventArgs message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var payload = Encoding.UTF8.GetString(message.ApplicationMessage.PayloadSegment);
        _logger.LogInformation("Received message: {payload} from topic: {message.ApplicationMessage.Topic}", payload, message.ApplicationMessage.Topic);

        var mwBotMessage = JsonSerializer.Deserialize<MqttClientMessage>(payload);

        if (mwBotMessage is null || mwBot is null)
        {
            _logger.LogWarning("Invalid message received or MwBot not initialized");
            return;
        }

        try
        {
            _logger.BeginScope("Updating MwBot");
            mwBot.Status = mwBotMessage.Status;
            mwBot.BatteryPercentage = mwBotMessage.BatteryPercentage;

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var mwBotRepository = scope.ServiceProvider.GetRequiredService<MwBotRepository>();
                await mwBotRepository.UpdateAsync(mwBot);
            }

            if (mwBotMessage.Status == MwBotStatus.StandBy && HandlingRequest != null)
            {
                // Start the charging process after receiving confirmation
                await SimulateChargingProcess(mwBotMessage);
            }

            _logger.LogInformation("MwBot updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing received message: {message}", message);
        }
    }

    private async Task Re

    /// <summary>
    /// Send message to MQTT server/broker
    /// </summary>
    /// <param name="mwBotMessage"></param>
    /// <returns></returns>
    public async Task PublishClientMessageAsync(MqttClientMessage mwBotMessage)
    {
        var payload = JsonSerializer.Serialize(mwBotMessage);
        await PublishAsync(payload);
    }

    public async Task<bool> ConnectAsync()
    {
        try
        {
            _logger.BeginScope("Connecting to MQTT server");
            await _mqttClient.ConnectAsync(_options);
            _logger.LogInformation("Connected to MQTT server");
            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Timeout while connecting.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to MQTT server");
            return false;
        }
    }

    public async Task<bool> DisconnectAsync()
    {
        if (mwBot is null)
        {
            _logger.LogWarning("MwBot not initialized, cannot disconnect");
            return false;
        }

        if (mwBot.Status == MwBotStatus.Offline)
        {
            _logger.LogWarning("MwBot is not online, cannot disconnect");
            return false;
        }

        try
        {
            _logger.BeginScope("Disconnecting from MQTT server");
            mwBot.Status = MwBotStatus.Offline;

            using var scope = _serviceScopeFactory.CreateScope();
            var mwBotRepository = scope.ServiceProvider.GetRequiredService<MwBotRepository>();
            await mwBotRepository.UpdateAsync(mwBot);

            await _mqttClient.DisconnectAsync();
            _logger.LogInformation("Disconnected from MQTT server");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from MQTT server");
            return false;
        }
    }
}
