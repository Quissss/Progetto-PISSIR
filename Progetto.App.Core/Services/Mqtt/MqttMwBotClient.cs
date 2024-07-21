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
using Timer = System.Timers.Timer;

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
    private Timer _timer;
    private Timer _reconnectTimer;
    private Timer _batteryMonitorTimer;
    public MwBot? MwBot { get; private set; }
    public ImmediateRequest? HandlingRequest { get; private set; }
    // TODO: add mqttMessage as property here

    public MqttMwBotClient(ILogger<MqttMwBotClient> logger, IServiceScopeFactory serviceScopeFactory, ChargeManager chargeManager)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _chargeManager = chargeManager;

        _mqttClient = new MqttFactory().CreateMqttClient();
        _mqttClient.ApplicationMessageReceivedAsync += HandleReceivedApplicationMessage;

        // Set timer for processing charge requests
        _timer = new Timer(1000);
        _timer.Elapsed += async (sender, e) => await TimedProcessChargeRequest(sender);

        // Set timer for reconnect attempts
        _reconnectTimer = new Timer(5000); // Try to reconnect every 5 seconds
        _reconnectTimer.Elapsed += async (sender, e) => await AttemptReconnectAsync();

        // Set timer for monitoring battery
        _batteryMonitorTimer = new Timer(60000); // Check battery level every 60 seconds
        _batteryMonitorTimer.Elapsed += async (sender, e) => await CheckBatteryLevelAsync();
        _batteryMonitorTimer.Start();
    }

    private async Task CheckBatteryLevelAsync()
    {
        _timer.Stop();
        if (MwBot == null)
        {
            _logger.LogWarning("MwBot not initialized, cannot check battery level");
            return;
        }

        if (MwBot.BatteryPercentage < 20) // Threshold for low battery
        {
            _logger.LogInformation("MwBot {id} battery is low: {batteryPercentage}%", MwBot.Id, MwBot.BatteryPercentage);

            var mwBotMessage = new MqttClientMessage
            {
                MessageType = MessageType.RequestRecharge,
                Id = MwBot.Id,
                Status = MwBotStatus.MovingToDock,
                BatteryPercentage = MwBot.BatteryPercentage
            };

            await PublishClientMessageAsync(mwBotMessage);
        }
    }

    private async Task AttemptReconnectAsync()
    {
        if (_mqttClient.IsConnected)
            return;

        _logger.LogInformation("Attempting to reconnect to MQTT broker...");
        var isConnected = await ConnectAsync();
        if (isConnected)
        {
            _logger.LogInformation("Reconnected to MQTT broker successfully.");
            _reconnectTimer.Stop();
            await RecoverStateAsync();
        }
    }

    private async Task RecoverStateAsync()
    {
        if (MwBot is null)
            return;

        await InitializeMwBot(MwBot.Id);
        if (HandlingRequest is not null)
        {
            // Resume any ongoing charging
            await ProcessChargeRequest();
        }
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
            .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(5))
            .WithCleanSession()
            .Build();

        await InitializeMwBot(mwBotId);
        var isConnected = await ConnectAsync();
        if (isConnected)
        {
            await _mqttClient.SubscribeAsync($"mwbot{mwBotId}");
            _timer.Interval = 10 * 1000; // TODO: Move to const
            _timer.Start();
            _reconnectTimer.Start();
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
                MwBot = await mwBotRepository.GetByIdAsync(mwBotId.Value);

            if (MwBot is null)
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
            .WithTopic("toServer")
            .Build();

        if (!_mqttClient.IsConnected)
        {
            _logger.LogWarning("Client is not connected, cannot publish message");
            return;
        }

        await _mqttClient.PublishAsync(message);
        _logger.LogDebug("Published message {payload}", payload);
    }

    private async Task<bool> ChangeBotStatus(MwBotStatus status)
    {
        if (MwBot is null)
        {
            _logger.LogWarning("MwBot not initialized, cannot change status");
            return false;
        }

        switch (status)
        {
            case MwBotStatus.Offline:
                _logger.LogInformation("MwBot {id}: Disconnecting from MQTT server", MwBot.Id);
                await DisconnectAsync();
                break;
            case MwBotStatus.StandBy:
                _logger.LogInformation("MwBot {id}: Awaiting for task", MwBot.Id);
                break;
            case MwBotStatus.ChargingCar:
                _logger.LogInformation("MwBot {id}: Charging car", MwBot.Id);
                break;
            case MwBotStatus.Recharging:
                _logger.LogInformation("MwBot {id}: Recharging", MwBot.Id);
                break;
            case MwBotStatus.MovingToSlot:
                _logger.LogInformation("MwBot {id}: Going to charge car on parking slot", MwBot.Id);
                break;
            case MwBotStatus.MovingToDock:
                _logger.LogInformation("MwBot {id}: Going to dock for recharge", MwBot.Id);
                break;
            default:
                _logger.LogWarning("MwBot {id}: Invalid status", MwBot.Id);
                return false;
        }

        MwBot.Status = status;
        var mwBotMessage = new MqttClientMessage
        {
            MessageType = MessageType.UpdateMwBot,
            Id = MwBot.Id,
            Status = MwBot.Status,
            BatteryPercentage = MwBot.BatteryPercentage,
            ParkingSlotId = HandlingRequest?.ParkingSlotId,
            TargetBatteryPercentage = HandlingRequest?.RequestedChargeLevel,
            UserId = HandlingRequest?.UserId,
        };
        await PublishClientMessageAsync(mwBotMessage);

        return true;
    }
    public async Task StartRechargingAsync()
    {
        if (MwBot == null)
        {
            _logger.LogWarning("MwBot not initialized, cannot start recharging");
            return;
        }

        // Simulate movement to the docking station
        await SimulateMovement(MwBotStatus.MovingToDock);

        // Simulate recharging process
        await SimulateRechargingProcessAsync();
    }

    private async Task SimulateRechargingProcessAsync()
    {
        if (MwBot == null)
        {
            _logger.LogWarning("MwBot not initialized, cannot recharge");
            return;
        }

        decimal rechargeRate = 2.0m; // kW/s

        // Set start time and initial battery percentage
        var startTime = DateTime.Now;
        decimal initialBatteryPercentage = MwBot.BatteryPercentage;

        while (MwBot.BatteryPercentage < 100)
        {
            await Task.Delay(1000); // Simulate one second

            MwBot.BatteryPercentage += rechargeRate;
            if (MwBot.BatteryPercentage > 100)
            {
                MwBot.BatteryPercentage = 100;
            }

            // Publish updated status
            var mwBotMessage = new MqttClientMessage
            {
                MessageType = MessageType.UpdateMwBot,
                Id = MwBot.Id,
                Status = MwBotStatus.Recharging,
                BatteryPercentage = MwBot.BatteryPercentage,
            };
            await PublishClientMessageAsync(mwBotMessage);

            _logger.LogInformation("MwBot {id} recharging: {batteryPercentage}%", MwBot.Id, MwBot.BatteryPercentage);
        }

        _logger.LogInformation("MwBot {id} completed recharging", MwBot.Id);

        // Change status to StandBy after recharging
        await ChangeBotStatus(MwBotStatus.StandBy);
        _timer.Start();
    }

    /// <summary>
    /// EVENT: What to do every _timer interval
    /// </summary>
    /// <param name="state"></param>
    private async Task TimedProcessChargeRequest(object? state)
    {
        if (MwBot?.Status == MwBotStatus.StandBy)
        {
            // TODO: recover charge
            HandlingRequest = await _chargeManager.ServeNext(MwBot);
            if (HandlingRequest is not null)
            {
                _logger.BeginScope("Processing request {request}", HandlingRequest);
                _timer.Stop();
                await SimulateMovement(MwBotStatus.MovingToSlot);
                await ProcessChargeRequest();
            }
        }
    }

    private async Task SimulateMovement(MwBotStatus status)
    {
        int minDelaySeconds = 5; // minimum seconds
        int maxDelaySeconds = 10; // max seconds
        int delayRange = maxDelaySeconds - minDelaySeconds;
        int delay = minDelaySeconds + (int)(delayRange * (1 - MwBot.BatteryPercentage / 100));

        if (status == MwBotStatus.MovingToSlot)
        {
            _logger.LogInformation("Simulating movement to car {delay} seconds...", delay);
        }
        else if (status == MwBotStatus.MovingToDock)
        {
            _logger.LogInformation("Simulating movement to dock {delay} seconds...", delay);
        }
        else
        {
            _logger.LogWarning("Invalid status");
            return;
        }

        var changeSuccess = await ChangeBotStatus(status);
        if (!changeSuccess)
            return;

        await Task.Delay(TimeSpan.FromSeconds(delay));
    }

    /// <summary>
    /// Send request to broker to start charging process
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task ProcessChargeRequest()
    {
        if (MwBot is null)
        {
            _logger.LogWarning("MwBot not initialized, cannot process request");
            return;
        }

        if (!_mqttClient.IsConnected)
        {
            _logger.LogWarning("Client is not connected, cannot publish message");
            return;
        }

        var mwBotMessage = new MqttClientMessage
        {
            MessageType = MessageType.RequestCharge,
            Id = MwBot.Id,
            Status = MwBot.Status,
            BatteryPercentage = MwBot.BatteryPercentage,
            ParkingId = MwBot.ParkingId,
            ParkingSlotId = HandlingRequest?.ParkingSlotId,
            TargetBatteryPercentage = HandlingRequest?.RequestedChargeLevel,
            UserId = HandlingRequest?.UserId,
        };

        await PublishClientMessageAsync(mwBotMessage);
    }

    private async Task SimulateChargingProcess(MqttClientMessage mwBotMessage)
    {
        decimal chargeRate = 0.5m; // kW/s
        decimal dischargeRate = 0.3m; // kW/s
        decimal energyCostPerKw = mwBotMessage.Parking?.EnergyCostPerKw ?? 0.2m;
        decimal stopCostPerMinute = mwBotMessage.Parking?.StopCostPerMinute ?? 0.1m;

        var changeSuccess = await ChangeBotStatus(MwBotStatus.ChargingCar);
        if (!changeSuccess)
            return;

        // Set random start charge percentage
        Random random = new Random();
        decimal minValue = 1;
        decimal maxValue = mwBotMessage.TargetBatteryPercentage ?? 50;
        decimal randomStartCharge = random.Next((int)minValue, (int)maxValue);

        // Set start time and intial charge percentage
        var currentlyCharging = mwBotMessage.CurrentlyCharging;
        var startTime = currentlyCharging.StartChargingTime = DateTime.Now;
        currentlyCharging.StartChargePercentage = currentlyCharging.CurrentChargePercentage = mwBotMessage.CurrentCarCharge = randomStartCharge;

        mwBotMessage.MessageType = MessageType.UpdateCharging;
        currentlyCharging.StartChargePercentage = currentlyCharging.StartChargePercentage = Math.Round(currentlyCharging.StartChargePercentage ?? 0, 2);
        await PublishClientMessageAsync(mwBotMessage);

        _logger.LogInformation("MwBot {id}: [{startDate}] Starting charge from {startingBattery}% to {targetBattery}%", mwBotMessage.Id, startTime, randomStartCharge, currentlyCharging.TargetChargePercentage);

        // TODO: non far arrivare a 0 mwbot perchè non siamo stronzi
        while (mwBotMessage.CurrentCarCharge < mwBotMessage.TargetBatteryPercentage && MwBot.BatteryPercentage > 0)
        {
            await Task.Delay(1000); // Simulate one second

            mwBotMessage.CurrentCarCharge += chargeRate;
            MwBot.BatteryPercentage -= dischargeRate;

            if (mwBotMessage.CurrentCarCharge > mwBotMessage.TargetBatteryPercentage)
            {
                mwBotMessage.CurrentCarCharge = mwBotMessage.TargetBatteryPercentage;
            }

            currentlyCharging.EnergyConsumed += chargeRate / 3600; // Convert kW/s to kWh

            var elapsedMinutes = (DateTime.Now - startTime).Value.TotalMinutes;
            currentlyCharging.TotalCost = (currentlyCharging.EnergyConsumed * energyCostPerKw) + ((decimal)elapsedMinutes * stopCostPerMinute);

            _logger.LogInformation("MwBot {botId}: Charging car: {carCharge}% / {requestedCharge}% | MwBot battery: {botBattery}% | Energy consumed: {energyConsumed:F7} kWh | Total cost: {totalCost:F2} EUR",
                MwBot.Id, mwBotMessage.CurrentCarCharge, mwBotMessage.TargetBatteryPercentage, MwBot.BatteryPercentage, currentlyCharging.EnergyConsumed, currentlyCharging.TotalCost);

            currentlyCharging.CurrentChargePercentage = mwBotMessage.CurrentCarCharge;
            currentlyCharging.CurrentChargePercentage = Math.Round(currentlyCharging.CurrentChargePercentage.Value, 2);
            currentlyCharging.EnergyConsumed = Math.Round(currentlyCharging.EnergyConsumed, 7);
            currentlyCharging.TotalCost = Math.Round(currentlyCharging.TotalCost, 2);
            mwBotMessage.CurrentlyCharging = currentlyCharging;
            mwBotMessage.BatteryPercentage = MwBot.BatteryPercentage;
            await PublishClientMessageAsync(mwBotMessage);
        }

        await CompleteChargingProcess(mwBotMessage);
    }

    private async Task CompleteChargingProcess(MqttClientMessage mwBotMessage)
    {
        var endTime = DateTime.Now;
        var currentlyCharging = mwBotMessage.CurrentlyCharging;
        var elapsedMinutes = (endTime - currentlyCharging.StartChargingTime).Value.TotalMinutes;
        currentlyCharging.TotalCost = (currentlyCharging.EnergyConsumed * mwBotMessage.Parking.EnergyCostPerKw) + ((decimal)elapsedMinutes * mwBotMessage.Parking.StopCostPerMinute);

        mwBotMessage.Status = MwBotStatus.StandBy;
        await ChangeBotStatus(MwBotStatus.StandBy);

        mwBotMessage = new MqttClientMessage
        {
            MessageType = MessageType.CompleteCharge,
            Id = MwBot.Id,
            Status = MwBot.Status,
            BatteryPercentage = MwBot.BatteryPercentage,
            ParkingSlotId = HandlingRequest.ParkingSlotId,
            TargetBatteryPercentage = HandlingRequest.RequestedChargeLevel,
            CurrentlyCharging = currentlyCharging
        };

        await PublishClientMessageAsync(mwBotMessage);
        _timer.Start();

        _logger.LogInformation("MwBot {botId}: Completed charging car at parking slot {slotId}. Total energy consumed: {energyConsumed:F7} kWh, Total cost: {totalCost:F2} EUR",
            MwBot?.Id, mwBotMessage.ParkingSlotId, currentlyCharging.EnergyConsumed, currentlyCharging.TotalCost);
    }

    /// <summary>
    /// Handle received application message from MQTT server
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private async Task HandleReceivedApplicationMessage(MqttApplicationMessageReceivedEventArgs arg)
    {
        ArgumentNullException.ThrowIfNull(arg);

        var payload = Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment);
        string topic = arg.ApplicationMessage.Topic;
        _logger.LogInformation("Received message: {payload} from topic: {message.ApplicationMessage.Topic}", payload, topic);

        var brokerMessage = JsonSerializer.Deserialize<MqttClientMessage>(payload);
        if (brokerMessage is null || MwBot is null)
        {
            _logger.LogWarning("Invalid message received");
            return;
        }

        if (MwBot is null)
        {
            _logger.LogWarning("MwBot not initialized");
            return;
        }

        try
        {
            MwBot.Status = brokerMessage.Status;
            MwBot.BatteryPercentage = brokerMessage.BatteryPercentage;

            var mwBotMessage = new MqttClientMessage
            {
                MessageType = MessageType.UpdateMwBot,
                Id = MwBot.Id,
                Status = MwBot.Status,
                BatteryPercentage = MwBot.BatteryPercentage,
            };
            await PublishClientMessageAsync(mwBotMessage);

            switch (brokerMessage.MessageType)
            {
                case MessageType.StartCharging:
                    await SimulateChargingProcess(brokerMessage);
                    break;
                case MessageType.StartRecharge:
                    await StartRechargingAsync();
                    break;
                default:
                    _logger.LogWarning("MwBot {id}: Invalid message type {type}", MwBot.Id, brokerMessage.MessageType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing received arg: {arg}", arg);
        }
    }

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
            _timer.Start();
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
        if (MwBot is null)
        {
            _logger.LogWarning("MwBot not initialized, cannot disconnect");
            return false;
        }

        if (MwBot.Status == MwBotStatus.Offline)
        {
            _logger.LogWarning("MwBot is not online, cannot disconnect");
            return false;
        }

        try
        {
            _logger.BeginScope("Disconnecting from MQTT server");
            _timer.Stop();

            MwBot.Status = MwBotStatus.Offline;
            var mwBotMessage = new MqttClientMessage
            {
                MessageType = MessageType.UpdateMwBot,
                Id = MwBot.Id,
                Status = MwBot.Status,
                BatteryPercentage = MwBot.BatteryPercentage,
            };
            await PublishClientMessageAsync(mwBotMessage);

            if (HandlingRequest?.ParkingSlot is not null)
            {
                HandlingRequest.ParkingSlot.Status = ParkingSlotStatus.Free;
                mwBotMessage.MessageType = MessageType.UpdateParkingSlot;
                mwBotMessage.ParkingSlot = HandlingRequest.ParkingSlot;
                await PublishClientMessageAsync(mwBotMessage);
            }

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
