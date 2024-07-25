using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Progetto.App.Core.Models;
using Progetto.App.Core.Models.Mqtt;
using Progetto.App.Core.Repositories;
using System.Text;
using System.Text.Json;
using System.Threading;
using Timer = System.Timers.Timer;

namespace Progetto.App.Core.Services.Mqtt;

/// <summary>
/// MwBot MQTT client (curresponds to single instance of mwbot when it's online)
/// </summary>
public class MqttMwBotClient
{
    private MqttClientOptions? _options;
    private readonly IMqttClient _mqttClient;
    private readonly ILogger<MqttMwBotClient> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ChargeManager _chargeManager; // TODO: delegate to broker
    private const int _rechargeThreshold = 5; // Battery percentage threshold for recharging
    private Timer _timer;
    private Timer _reconnectTimer;
    private Timer _batteryMonitorTimer;
    private CancellationTokenSource _cancellationTokenSource;
    private CancellationToken _cancellationToken;
    private const int _chargeDelay = 1000;
    private const int _rechargeDelay = 1000;
    // TODO: add mqttMessage as property here and handle it
    public MwBot? MwBot { get; private set; }
    public ImmediateRequest? HandlingRequest { get; private set; }

    public MqttMwBotClient(ILogger<MqttMwBotClient> logger, IServiceScopeFactory serviceScopeFactory, ChargeManager chargeManager)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _chargeManager = chargeManager;
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;

        _mqttClient = new MqttFactory().CreateMqttClient();
        _mqttClient.ApplicationMessageReceivedAsync += async (e) => await HandleReceivedApplicationMessage(e, _cancellationToken);

        // Set timer for processing charge requests
        _timer = new Timer(3000);
        _timer.Elapsed += async (sender, e) => await TimedProcessChargeRequest(sender, _cancellationToken);

        // Set timer for reconnect attempts
        _reconnectTimer = new Timer(5000);
        _reconnectTimer.Elapsed += async (sender, e) => await AttemptReconnectAsync(_cancellationToken);

        // Set timer for monitoring battery
        _batteryMonitorTimer = new Timer(10000);
        _batteryMonitorTimer.Elapsed += async (sender, e) => await CheckBatteryLevelAsync();
    }

    private void ResetCancellationToken()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
    }

    private async Task RequestResumeChargingAsync()
    {
        if (MwBot == null)
        {
            _logger.LogWarning("MwBot not initialized, cannot request to resume charging");
            return;
        }

        var mwBotMessage = new MqttClientMessage
        {
            MessageType = MessageType.RequestResumeCharging,
            Id = MwBot.Id,
            Status = MwBot.Status,
            BatteryPercentage = MwBot.BatteryPercentage,
            ImmediateRequestId = HandlingRequest?.Id,
            ParkingSlotId = HandlingRequest?.ParkingSlotId,
            TargetBatteryPercentage = HandlingRequest?.RequestedChargeLevel,
            UserId = HandlingRequest?.UserId,
            ParkingId = MwBot.ParkingId,
            CarPlate = HandlingRequest?.CarPlate,
            Parking = MwBot.Parking,
        };
        await PublishClientMessageAsync(mwBotMessage, _cancellationTokenSource.Token);
    }

    private async Task CheckBatteryLevelAsync()
    {
        if (MwBot == null)
        {
            _logger.LogWarning("MwBot not initialized, cannot check battery level");
            return;
        }

        // TODO: FIX if bot battery starts 5% it stays still
        if (MwBot.BatteryPercentage <= _rechargeThreshold) // Threshold for low battery
        {
            _logger.LogInformation("MwBot {id} battery is low: {batteryPercentage}%", MwBot.Id, MwBot.BatteryPercentage);
            if (_batteryMonitorTimer.Enabled) _batteryMonitorTimer.Stop();
            if (_timer.Enabled) _timer.Stop();

            var mwBotMessage = new MqttClientMessage
            {
                MessageType = MessageType.RequestRecharge,
                Id = MwBot.Id,
                Status = MwBotStatus.MovingToDock,
                BatteryPercentage = MwBot.BatteryPercentage,
                ParkingSlotId = HandlingRequest?.ParkingSlotId,
                TargetBatteryPercentage = HandlingRequest?.RequestedChargeLevel,
                UserId = HandlingRequest?.UserId,
                CarPlate = HandlingRequest?.CarPlate,
                ParkingId = MwBot.ParkingId,
                ImmediateRequestId = HandlingRequest?.Id,
                Parking = MwBot.Parking,
            };
            await PublishClientMessageAsync(mwBotMessage, _cancellationTokenSource.Token);
        }
    }

    private async Task AttemptReconnectAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient.IsConnected)
            return;

        _logger.LogInformation("MwBot {id}: Attempting to reconnect to MQTT broker...", MwBot?.Id);
        var isConnected = await ConnectAsync();
        if (isConnected)
        {
            _logger.LogInformation("MwBot {id}: Reconnected to MQTT broker successfully.", MwBot?.Id);
            await RecoverStateAsync(cancellationToken);
        }
        else
        {
            await Task.Delay(5000, cancellationToken);
            await AttemptReconnectAsync(cancellationToken);
        }
    }

    private async Task RecoverStateAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (MwBot is null)
            {
                _logger.LogWarning("MwBot not initialized, cannot process charge request");
                return;
            }
            else
            {
                await InitializeMwBot(MwBot.Id);
            }

            if (HandlingRequest is not null)
            {
                _logger.LogInformation("MwBot {id}: Recovering state. Resuming handling request {requestId}.", MwBot.Id, HandlingRequest.Id);
                await RequestResumeChargingAsync();
            }
            else if (MwBot.Status == MwBotStatus.StandBy && MwBot.BatteryPercentage > _rechargeThreshold)
            {
                _logger.LogInformation("MwBot {id} is in StandBy. Checking for new charge requests.", MwBot.Id);
                HandlingRequest = await _chargeManager.ServeNext(MwBot);
                if (HandlingRequest is not null)
                {
                    _logger.LogInformation("MwBot {id} found new request {requestId}.", MwBot.Id, HandlingRequest.Id);
                    await SimulateMovement(MwBotStatus.MovingToSlot, cancellationToken);
                    await ProcessChargeRequest();
                }
                else
                {
                    _logger.LogInformation("MwBot {id}: No charge requests available.", MwBot.Id);
                }
            }
            else
            {
                var mwBotMessage = new MqttClientMessage
                {
                    MessageType = MessageType.RequestResumeCharging,
                    Id = MwBot.Id,
                    Status = MwBot.Status,
                    BatteryPercentage = MwBot.BatteryPercentage,
                    ImmediateRequestId = HandlingRequest?.Id,
                    ParkingSlotId = HandlingRequest?.ParkingSlotId,
                    TargetBatteryPercentage = HandlingRequest?.RequestedChargeLevel,
                    UserId = HandlingRequest?.UserId,
                    ParkingId = MwBot.ParkingId,
                    CarPlate = HandlingRequest?.CarPlate,
                    Parking = MwBot.Parking,
                };
                await PublishClientMessageAsync(mwBotMessage, _cancellationTokenSource.Token);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing charge request");
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
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(120)) // for debug purposes
            .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
            .WithCleanSession()
            .Build();

        var isConnected = await ConnectAsync();
        if (isConnected)
        {
            await _mqttClient.SubscribeAsync($"mwbot{mwBotId}");
            await InitializeMwBot(mwBotId);
            if (!_timer.Enabled) _timer.Start();
            if (!_batteryMonitorTimer.Enabled) _batteryMonitorTimer.Start();
            if (!_reconnectTimer.Enabled) _reconnectTimer.Start();
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
            // TODO: delegate to broker
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

        try
        {
            var message = new MqttApplicationMessageBuilder()
                .WithPayload(payload)
                .WithTopic("toServer")
                .Build();

            if (!_mqttClient.IsConnected)
            {
                _logger.LogWarning("MwBot {id}: Client is not connected, cannot publish message", MwBot?.Id);
                return;
            }

            await _mqttClient.PublishAsync(message);
            _logger.LogDebug("MwBot {id}: Published message {payload}", MwBot?.Id, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message {payload}", payload);
        }
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
                if (!_timer.Enabled) _timer.Start();
                break;
            case MwBotStatus.ChargingCar:
                _logger.LogInformation("MwBot {id}: Charging car", MwBot.Id);
                break;
            case MwBotStatus.Recharging:
                _logger.LogInformation("MwBot {id}: Recharging", MwBot.Id);
                break;
            case MwBotStatus.MovingToSlot:
                _logger.LogInformation("MwBot {id}: Going to charge car on parking slot {parkingSlot}", MwBot.Id, HandlingRequest?.ParkingSlotId);
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
            CarPlate = HandlingRequest?.CarPlate,
            ImmediateRequestId = HandlingRequest?.Id,
            ParkingId = MwBot.ParkingId,
            Parking = MwBot.Parking,
        };
        await PublishClientMessageAsync(mwBotMessage, _cancellationTokenSource.Token);

        return true;
    }

    public async Task StartRechargingAsync(CancellationToken cancellationToken)
    {
        if (MwBot == null)
        {
            _logger.LogWarning("MwBot not initialized, cannot start recharging");
            return;
        }

        await SimulateMovement(MwBotStatus.MovingToDock, cancellationToken);
        await SimulateRechargingProcessAsync(cancellationToken);
    }

    private async Task SimulateRechargingProcessAsync(CancellationToken cancellationToken)
    {
        if (MwBot == null)
        {
            _logger.LogWarning("MwBot not initialized, cannot recharge");
            return;
        }

        decimal rechargeRate = 2.0m; // kW/s
        var startTime = DateTime.Now;
        decimal initialBatteryPercentage = MwBot.BatteryPercentage;

        while (MwBot.BatteryPercentage < 100)
        {
            await Task.Delay(_rechargeDelay, cancellationToken); // Simulate one second

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Recharging process cancelled for MwBot {id}", MwBot.Id);
                return;
            }

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
                ParkingSlotId = HandlingRequest?.ParkingSlotId,
                TargetBatteryPercentage = HandlingRequest?.RequestedChargeLevel,
                UserId = HandlingRequest?.UserId,
                ImmediateRequestId = HandlingRequest?.Id,
                ParkingId = MwBot.ParkingId,
                Parking = MwBot.Parking,
            };
            await PublishClientMessageAsync(mwBotMessage, cancellationToken);

            _logger.LogInformation("MwBot {id} recharging: {batteryPercentage}%", MwBot.Id, MwBot.BatteryPercentage);
        }

        _logger.LogInformation("MwBot {id} completed recharging", MwBot.Id);

        // Change status to StandBy after recharging
        await ChangeBotStatus(MwBotStatus.StandBy);
        await RequestResumeChargingAsync();
    }

    /// <summary>
    /// EVENT: What to do every _timer interval
    /// </summary>
    /// <param name="state"></param>
    private async Task TimedProcessChargeRequest(object? state, CancellationToken cancellationToken)
    {
        try
        {
            if (MwBot is null)
            {
                _logger.LogWarning("MwBot not initialized, cannot process charge request");
                return;
            }

            // TODO: FIX resume recharge when status is StandBy (1) if there is one
            if (HandlingRequest is not null)
            {
                _logger.LogInformation("MwBot {id}: Resuming request {request}", MwBot.Id, HandlingRequest);
                if (_timer.Enabled) _timer.Stop();
                await RequestResumeChargingAsync();
            }
            else if (MwBot?.Status == MwBotStatus.StandBy && MwBot.BatteryPercentage > _rechargeThreshold)
            {
                HandlingRequest = await _chargeManager.ServeNext(MwBot);
                if (HandlingRequest is not null)
                {
                    _logger.BeginScope("MwBot {id}: Processing request {request}", MwBot.Id, HandlingRequest);
                    if (_timer.Enabled) _timer.Stop();

                    await SimulateMovement(MwBotStatus.MovingToSlot, cancellationToken);
                    await ProcessChargeRequest();
                }
            }
            else
            {
                var mwBotMessage = new MqttClientMessage
                {
                    MessageType = MessageType.RequestResumeCharging,
                    Id = MwBot.Id,
                    Status = MwBot.Status,
                    BatteryPercentage = MwBot.BatteryPercentage,
                    ImmediateRequestId = HandlingRequest?.Id,
                    ParkingSlotId = HandlingRequest?.ParkingSlotId,
                    TargetBatteryPercentage = HandlingRequest?.RequestedChargeLevel,
                    UserId = HandlingRequest?.UserId,
                    ParkingId = MwBot.ParkingId,
                    CarPlate = HandlingRequest?.CarPlate,
                    Parking = MwBot.Parking,
                };
                await PublishClientMessageAsync(mwBotMessage, _cancellationTokenSource.Token);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing charge request");
        }
    }

    private async Task SimulateMovement(MwBotStatus status, CancellationToken cancellationToken)
    {
        int minDelaySeconds = 5; // minimum seconds
        int maxDelaySeconds = 10; // max seconds
        int delayRange = maxDelaySeconds - minDelaySeconds;
        int delay = minDelaySeconds + (int)(delayRange * (1 - MwBot.BatteryPercentage / 100));

        if (status == MwBotStatus.MovingToSlot)
        {
            _logger.LogInformation("MwBot {id}: Simulating movement to car {delay} seconds...", MwBot.Id, delay);
        }
        else if (status == MwBotStatus.MovingToDock)
        {
            _logger.LogInformation("MwBot {id}: Simulating movement to dock {delay} seconds...", MwBot.Id, delay);
        }
        else
        {
            _logger.LogWarning("Invalid status");
            return;
        }

        var changeSuccess = await ChangeBotStatus(status);
        if (!changeSuccess)
            return;

        await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);
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
            _logger.LogWarning("MwBot {id}: Client is not connected, cannot publish message", MwBot.Id);
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
            ImmediateRequestId = HandlingRequest?.Id,
            Parking = MwBot.Parking,
            CarPlate = HandlingRequest?.CarPlate,
            ImmediateRequest = HandlingRequest,
        };

        await PublishClientMessageAsync(mwBotMessage, _cancellationTokenSource.Token);
    }

    private async Task SimulateChargingProcess(MqttClientMessage mwBotMessage, CancellationToken cancellationToken)
    {
        decimal chargeRate = 0.3m; // kW/s
        decimal dischargeRate = 0.5m; // kW/s
        decimal energyCostPerKw = mwBotMessage.Parking?.EnergyCostPerKw ?? 0.2m;
        decimal stopCostPerMinute = mwBotMessage.Parking?.StopCostPerMinute ?? 0.1m;

        var changeSuccess = await ChangeBotStatus(MwBotStatus.ChargingCar);
        if (!changeSuccess)
            return;

        // Set start time and intial charge percentage
        var startTime = DateTime.Now;
        var currentlyCharging = mwBotMessage.CurrentlyCharging;
        mwBotMessage.MessageType = MessageType.UpdateCharging;

        if (currentlyCharging.StartChargePercentage == 0 && currentlyCharging.CurrentChargePercentage == 0)
        {
            // Set random start charge percentage
            Random random = new Random();
            decimal minValue = 1;
            decimal maxValue = mwBotMessage.TargetBatteryPercentage ?? 50;
            decimal randomStartCharge = random.Next((int)minValue, (int)maxValue);

            currentlyCharging.StartChargingTime = startTime;
            currentlyCharging.StartChargePercentage = currentlyCharging.CurrentChargePercentage = mwBotMessage.CurrentCarCharge = randomStartCharge;

            currentlyCharging.StartChargePercentage = Math.Round(currentlyCharging.StartChargePercentage ?? 0, 2);
            await PublishClientMessageAsync(mwBotMessage, _cancellationTokenSource.Token);

            _logger.LogInformation("[{startDate}] MwBot {id}: Starting charge from {startingBattery}% to {targetBattery:F0}%", startTime, mwBotMessage.Id, randomStartCharge, currentlyCharging.TargetChargePercentage);
        }
        else
        {
            mwBotMessage.CurrentCarCharge = currentlyCharging.CurrentChargePercentage;
            _logger.LogInformation("[{startDate}] MwBot {id}: Resuming charge from {currentBattery}% to {targetBattery:F0}%", DateTime.Now, mwBotMessage.Id, currentlyCharging.CurrentChargePercentage, currentlyCharging.TargetChargePercentage);
        }

        while (mwBotMessage.CurrentCarCharge < mwBotMessage.TargetBatteryPercentage && MwBot.BatteryPercentage > _rechargeThreshold)
        {
            await Task.Delay(_chargeDelay, cancellationToken); // Simulate one second

            mwBotMessage.CurrentCarCharge += chargeRate;
            MwBot.BatteryPercentage -= dischargeRate;

            if (mwBotMessage.CurrentCarCharge > mwBotMessage.TargetBatteryPercentage)
            {
                mwBotMessage.CurrentCarCharge = mwBotMessage.TargetBatteryPercentage;
            }

            currentlyCharging.EnergyConsumed += chargeRate / 3600; // Convert kW/s to kWh

            var elapsedMinutes = (DateTime.Now - startTime).TotalMinutes;
            currentlyCharging.TotalCost = currentlyCharging.EnergyConsumed * energyCostPerKw + (decimal)elapsedMinutes * stopCostPerMinute;

            _logger.LogInformation("MwBot {botId}: Charging car: {carCharge}% / {requestedCharge}% | MwBot battery: {botBattery}% | Energy consumed: {energyConsumed:F7} kWh | Total cost: {totalCost:F2} EUR",
                MwBot.Id, mwBotMessage.CurrentCarCharge, mwBotMessage.TargetBatteryPercentage, MwBot.BatteryPercentage, currentlyCharging.EnergyConsumed, currentlyCharging.TotalCost);

            currentlyCharging.CurrentChargePercentage = mwBotMessage.CurrentCarCharge;
            currentlyCharging.CurrentChargePercentage = Math.Round(currentlyCharging.CurrentChargePercentage.Value, 2);
            currentlyCharging.EnergyConsumed = Math.Round(currentlyCharging.EnergyConsumed, 7);
            currentlyCharging.TotalCost = Math.Round(currentlyCharging.TotalCost, 2);

            mwBotMessage.CurrentlyCharging = currentlyCharging;
            mwBotMessage.BatteryPercentage = MwBot.BatteryPercentage;

            await PublishClientMessageAsync(mwBotMessage, _cancellationTokenSource.Token);
        }

        mwBotMessage.BatteryPercentage = MwBot.BatteryPercentage;
        await PublishClientMessageAsync(mwBotMessage, _cancellationTokenSource.Token);

        if (mwBotMessage.CurrentCarCharge >= mwBotMessage.TargetBatteryPercentage)
        {
            _logger.LogInformation("MwBot {botId}: Car charged to {targetBattery}%", MwBot.Id, mwBotMessage.TargetBatteryPercentage);
            await CompleteChargingProcess(mwBotMessage);
            if (!_timer.Enabled) _timer.Start();
        }
        else
        {
            _logger.LogInformation("MwBot {botId}: Stopping charging process due to low battery", MwBot.Id);
        }
    }

    private async Task CompleteChargingProcess(MqttClientMessage mwBotMessage)
    {
        var endTime = DateTime.Now;
        var currentlyCharging = mwBotMessage.CurrentlyCharging;
        var elapsedMinutes = (endTime - currentlyCharging.StartChargingTime).Value.TotalMinutes;
        currentlyCharging.TotalCost = currentlyCharging.EnergyConsumed * mwBotMessage.Parking.EnergyCostPerKw + (decimal)elapsedMinutes * mwBotMessage.Parking.StopCostPerMinute;

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
            CurrentlyCharging = currentlyCharging,
            UserId = HandlingRequest.UserId,
            ParkingId = MwBot.ParkingId,
            ImmediateRequestId = HandlingRequest.Id,
            Parking = MwBot.Parking,
            CarPlate = HandlingRequest.CarPlate,
        };

        await PublishClientMessageAsync(mwBotMessage, _cancellationTokenSource.Token);
        HandlingRequest = null;
        if (!_timer.Enabled) _timer.Start();

        _logger.LogInformation("MwBot {botId}: Completed charging car at parking slot {slotId}. Total energy consumed: {energyConsumed:F7} kWh, Total cost: {totalCost:F2} EUR",
            MwBot?.Id, mwBotMessage.ParkingSlotId, currentlyCharging.EnergyConsumed, currentlyCharging.TotalCost);
    }

    /// <summary>
    /// Handle received application message from MQTT server
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private async Task HandleReceivedApplicationMessage(MqttApplicationMessageReceivedEventArgs arg, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(arg);

        var payload = Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment);
        string topic = arg.ApplicationMessage.Topic;
        _logger.LogDebug("MwBot {id}: Received message: {payload} from topic: {message.ApplicationMessage.Topic}", MwBot.Id, payload, topic);

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

            if (_timer.Enabled) _timer.Stop();
            if (_batteryMonitorTimer.Enabled) _batteryMonitorTimer.Stop();
            switch (brokerMessage.MessageType)
            {
                case MessageType.StartCharging:
                    _logger.LogInformation("MwBot {id}: Received MessageType.StartCharging", MwBot.Id);
                    if (!_timer.Enabled) _timer.Start();
                    break;
                case MessageType.ResumeCharging:
                    _logger.LogInformation("MwBot {id}: Received MessageType.ResumeCharging", MwBot.Id);

                    var previousMwBotStatus = MwBot.Status;
                    if (brokerMessage.ImmediateRequest is null)
                    {
                        await ChangeBotStatus(MwBotStatus.StandBy);
                        return;
                    }

                    HandlingRequest = brokerMessage.ImmediateRequest;
                    if (previousMwBotStatus == MwBotStatus.Recharging || previousMwBotStatus == MwBotStatus.MovingToDock)
                    {
                        await SimulateMovement(MwBotStatus.MovingToSlot, cancellationToken);
                    }
                    await SimulateChargingProcess(brokerMessage, cancellationToken);
                    break;
                case MessageType.StartRecharge:
                    _logger.LogInformation("MwBot {id}: Received MessageType.StartRecharge", MwBot.Id);
                    await StartRechargingAsync(cancellationToken);
                    break;
                default:
                    _logger.LogWarning("MwBot {id}: Invalid message type {type}", MwBot.Id, brokerMessage.MessageType);
                    break;
            }
            if (!_batteryMonitorTimer.Enabled) _batteryMonitorTimer.Start();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing received arg: {arg}", arg);
        }
    }

    public async Task PublishClientMessageAsync(MqttClientMessage mwBotMessage, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(mwBotMessage);
        await PublishAsync(payload);
        _logger.LogDebug("MwBot {id}: Published message {payload}", MwBot?.Id, payload);
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
        if (MwBot is null)
        {
            _logger.LogWarning("MwBot not initialized, cannot disconnect");
            return false;
        }

        if (MwBot.Status == MwBotStatus.Offline)
        {
            _logger.LogWarning("MwBot {id} is not online, cannot disconnect", MwBot.Id);
            return false;
        }

        try
        {
            _logger.BeginScope("MwBot {id}: Disconnecting from MQTT server", MwBot.Id);
            if (_timer.Enabled) _timer.Stop();
            if (_batteryMonitorTimer.Enabled) _batteryMonitorTimer.Stop();
            if (_reconnectTimer.Enabled) _reconnectTimer.Stop();
            ResetCancellationToken();

            MwBot.Status = MwBotStatus.Offline;
            var mwBotMessage = new MqttClientMessage
            {
                MessageType = MessageType.DisconnectClient,
                Id = MwBot.Id,
                Status = MwBot.Status,
                BatteryPercentage = MwBot.BatteryPercentage,
                TargetBatteryPercentage = HandlingRequest?.RequestedChargeLevel,
                UserId = HandlingRequest?.UserId,
                ImmediateRequestId = HandlingRequest?.Id,
                Parking = MwBot.Parking,
                ParkingId = MwBot.ParkingId,
                ParkingSlotId = HandlingRequest?.ParkingSlotId
            };

            await PublishClientMessageAsync(mwBotMessage, _cancellationTokenSource.Token);
            await _mqttClient.DisconnectAsync();
            _logger.LogInformation("MwBot {id}: Disconnected from MQTT server", MwBot.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from MQTT server");
            return false;
        }
    }
}
