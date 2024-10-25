using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Progetto.App.Core.Models;
using Progetto.App.Core.Models.Mqtt;
using System.Text;
using System.Text.Json;
using Timer = System.Timers.Timer;

namespace Progetto.App.Core.Services.Mqtt;

/// <summary>
/// MwBot MQTT client (curresponds to single instance of mwbot when it's online)
/// </summary>
public class MqttMwBotClient : IDisposable
{
    private readonly IMqttClient _mqttClient;
    private readonly ILogger<MqttMwBotClient> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private MqttClientOptions? _options;
    private MqttClientMessage MwBotMessage { get; set; } = new();
    private ImmediateRequest? HandlingRequest { get; set; }
    public MwBot? MwBot { get; set; }

    private CancellationTokenSource _cancellationTokenSource = new();

    // timers
    private readonly Timer _timer = new(1000);
    private readonly Timer _reconnectTimer = new(5000);

    // flags
    private bool _isConnecting = false;
    private bool _isCharging = false;
    private bool _isRecharging = false;

    // counters
    private int _chargeRequested = 0; // Number of consequent charge requests
    private const int _maxRequestsBeforeRecharge = 3; // Number of consequent charge requests before recharging

    // constants
    private const int _rechargeThreshold = 5; // Battery percentage threshold for recharge triggering
    private const int _maxRechargeThreshold = 80; // Maximum battery percentage threshold for recharge

    // delays
    private const int _chargeDelay = 1000;
    private const int _rechargeDelay = 1000;

    // locks and semaphores
    private readonly object _timerLock = new();
    private readonly object _lockMessage = new();
    private readonly SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(1, 1);

    // for hue lights
    private static readonly string baseUrl = "http://localhost:8000";
    private static readonly string username = "newdeveloper";
    private static readonly string lightsUrl = $"{baseUrl}/api/{username}/lights/";
    private static readonly HttpClient httpClient = new HttpClient();

    public MqttMwBotClient(ILogger<MqttMwBotClient> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;

        _mqttClient = new MqttFactory().CreateMqttClient();
        _mqttClient.ApplicationMessageReceivedAsync += async (e) => await HandleReceivedApplicationMessage(e, _cancellationTokenSource.Token);
        _mqttClient.DisconnectedAsync += async (e) => await HandleClientDisconnectedAsync(e);

        _timer.Elapsed += async (sender, e) => await TimedProcessChargeRequest(_cancellationTokenSource.Token);
        _reconnectTimer.Elapsed += async (sender, e) => await TimedAttemptReconnectAsync(_cancellationTokenSource.Token);
    }

    private bool _disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _timer?.Stop();
            _timer?.Dispose();

            _reconnectTimer?.Stop();
            _reconnectTimer?.Dispose();

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();

            _connectionSemaphore?.Dispose();

            if (_mqttClient != null)
            {
                if (_mqttClient.IsConnected)
                {
                    _mqttClient.DisconnectAsync().Wait();
                }
                _mqttClient.Dispose();
            }
        }

        _disposed = true;
    }

    ~MqttMwBotClient()
    {
        Dispose(false);
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

        var brokerMessage = JsonSerializer.Deserialize<MqttClientMessage>(payload);
        if (brokerMessage is null)
        {
            _logger.LogWarning("Invalid message received");
            return;
        }

        MwBot ??= new MwBot
        {
            Id = brokerMessage.Id,
            Status = brokerMessage.Status,
            BatteryPercentage = brokerMessage.BatteryPercentage,
            ParkingId = brokerMessage.ParkingId,
            Parking = brokerMessage.Parking
        };

        lock (_lockMessage)
        {
            MwBotMessage.Id = MwBot.Id = brokerMessage.Id;
            MwBotMessage.Status = MwBot.Status = brokerMessage.Status;
            MwBotMessage.BatteryPercentage = MwBot.BatteryPercentage = brokerMessage.BatteryPercentage;
            MwBotMessage.ParkingId = MwBot.ParkingId = brokerMessage.ParkingId;
            MwBotMessage.Parking = MwBot.Parking = brokerMessage.Parking;
            MwBotMessage.LatestLocation = MwBot.LatestLocation = brokerMessage.LatestLocation;
        }

        //_logger.LogDebug("MwBot {id}: Received message: {payload} from topic: {message.ApplicationMessage.Topic}", MwBot.Id, payload, topic);

        try
        {
            switch (brokerMessage.MessageType)
            {
                case MessageType.StartCharging:
                    _logger.LogInformation("MwBot {id}: Received MessageType.StartCharging", MwBot.Id);

                    if (_isCharging)
                    {
                        _logger.LogWarning("MwBot {id}: Charging process is already running. Ignoring StartCharging message.", MwBot.Id);
                        break;
                    }

                    if (brokerMessage.ImmediateRequest is null)
                    {
                        _logger.LogDebug("MwBot {id}: No immediate request found. Changing status to StandBy", MwBot.Id);
                        await ChangeBotStatus(MwBotStatus.StandBy);
                        break;
                    }

                    lock (_lockMessage) HandlingRequest = brokerMessage.ImmediateRequest;
                    lock (_timerLock) if (_timer.Enabled) _timer.Stop();
                    _isCharging = true;
                    _chargeRequested = 0;

                    if (brokerMessage.LatestLocation != MwBotLocations.InSlot)
                        await SimulateMovement(MwBotStatus.MovingToSlot, cancellationToken);

                    await ChangeBotLocation(MwBotLocations.InSlot);
                    _ = SimulateChargingProcess(brokerMessage, cancellationToken);
                    break;

                case MessageType.StartRecharge:
                    _logger.LogInformation("MwBot {id}: Received MessageType.StartRecharge", MwBot.Id);

                    if (_isRecharging)
                    {
                        _logger.LogWarning("MwBot {id}: Recharging process is already running. Ignoring StartRecharge message.", MwBot.Id);
                        break;
                    }

                    if (MwBot == null)
                    {
                        _logger.LogWarning("MwBot not initialized, cannot start recharging");
                        return;
                    }

                    lock (_timerLock) if (_timer.Enabled) _timer.Stop();

                    if (brokerMessage.LatestLocation != MwBotLocations.InDock)
                        await SimulateMovement(MwBotStatus.MovingToDock, cancellationToken);

                    await ChangeBotLocation(MwBotLocations.InDock);
                    await SimulateRechargingProcessAsync(cancellationToken);
                    _isRecharging = false;
                    break;

                case MessageType.ChargeCompleted:
                    _logger.LogInformation("MwBot {id}: Received MessageType.ChargeCompleted", MwBot.Id);
                    lock (_timerLock) if (!_timer.Enabled) _timer.Start();
                    break;

                case MessageType.StopCharging:
                    _logger.LogInformation("MwBot {id}: Received MessageType.StopCharging", MwBot.Id);
                    _isCharging = false;
                    break;

                case MessageType.ReturnMwBot:
                    _logger.LogInformation("MwBot {id}: Received MessageType.ReturnMwBot", MwBot.Id);
                    // Handled by default at the top of the switch
                    break;

                default:
                    _logger.LogWarning("MwBot {id}: Invalid message type {type}", MwBot.Id, brokerMessage.MessageType);
                    break;
            }

            lock (_lockMessage)
            {
                MwBotMessage.ParkingSlotId = HandlingRequest?.ParkingSlotId;
                MwBotMessage.TargetBatteryPercentage = HandlingRequest?.RequestedChargeLevel;
                MwBotMessage.UserId = HandlingRequest?.UserId;
                MwBotMessage.CarPlate = HandlingRequest?.CarPlate;
                MwBotMessage.ImmediateRequestId = HandlingRequest?.Id;
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Operazione annullata.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing received arg: {arg}", arg);
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
            lock (_timerLock)
            {
                if (!_timer.Enabled) _timer.Start();
                if (!_reconnectTimer.Enabled) _reconnectTimer.Start();
            }
        }
        return isConnected;
    }

    /// <summary>
    /// Initialize MwBot property with given id if exists in database
    /// </summary>
    /// <param name="mwBotId"></param>
    /// <returns></returns>
    private async Task InitializeMwBot(int? mwBotId)
    {
        if (mwBotId == null)
        {
            _logger.LogWarning("MwBot ID is null");
            return;
        }

        lock (_lockMessage)
        {
            MwBotMessage.MessageType = MessageType.RequestMwBot;
            MwBotMessage.Id = mwBotId.Value;
        }

        await PublishClientMessageAsync(MwBotMessage, _cancellationTokenSource.Token);
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
                await ChangeLightState(MwBot.Id.ToString(), "{ \"on\": false }");
                await DisconnectAsync();
                break;
            case MwBotStatus.StandBy:
                _logger.LogInformation("MwBot {id}: Awaiting for task", MwBot.Id);
                await ChangeLightState(MwBot.Id.ToString(), "{\"on\":true,\"sat\":0,\"bri\":254,\"hue\":0}");
                lock (_timerLock) if (!_timer.Enabled) _timer.Start();
                break;
            case MwBotStatus.ChargingCar:
                _logger.LogInformation("MwBot {id}: Charging car", MwBot.Id);
                await ChangeLightState(MwBot.Id.ToString(), "{\"on\":true,\"sat\":254,\"bri\":254,\"hue\":25500}");
                break;
            case MwBotStatus.Recharging:
                _logger.LogInformation("MwBot {id}: Recharging", MwBot.Id);
                await ChangeLightState(MwBot.Id.ToString(), "{\"on\":true,\"sat\":254,\"bri\":254,\"hue\":46920}");
                break;
            case MwBotStatus.MovingToSlot:
                _logger.LogInformation("MwBot {id}: Going to charge car on parking slot {parkingSlot}", MwBot.Id, HandlingRequest?.ParkingSlotId);
                await ChangeLightState(MwBot.Id.ToString(), "{\"on\":true,\"sat\":0,\"bri\":154,\"hue\":25500}");
                break;
            case MwBotStatus.MovingToDock: 
                _logger.LogInformation("MwBot {id}: Going to dock for recharge", MwBot.Id);
                await ChangeLightState(MwBot.Id.ToString(), "{\"on\":true,\"sat\":0,\"bri\":154,\"hue\":46920}");
                break;
            default:
                _logger.LogWarning("MwBot {id}: Invalid status", MwBot.Id);
                await ChangeLightState(MwBot.Id.ToString(), "{\"on\":true,\"sat\":254,\"bri\":254,\"hue\":0}");
                return false;
        }

        lock (_lockMessage)
        {
            MwBotMessage.MessageType = MessageType.UpdateMwBot;
            MwBotMessage.Status = MwBot.Status = status;
        }

        await PublishClientMessageAsync(MwBotMessage, _cancellationTokenSource.Token);

        return true;
    }

    private async Task<bool> ChangeBotLocation(MwBotLocations location)
    {
        if (MwBot is null)
        {
            _logger.LogWarning("MwBot not initialized, cannot change location");
            return false;
        }

        switch (location)
        {
            case MwBotLocations.InDock:
                _logger.LogInformation("MwBot {id}: In dock", MwBot.Id);
                break;
            case MwBotLocations.InSlot:
                _logger.LogInformation("MwBot {id}: In parking slot", MwBot.Id);
                break;
            default:
                _logger.LogWarning("MwBot {id}: Invalid location", MwBot.Id);
                return false;
        }

        lock (_lockMessage)
        {
            MwBotMessage.MessageType = MessageType.UpdateMwBot;
            MwBotMessage.LatestLocation = MwBot.LatestLocation = location;
        }

        await PublishClientMessageAsync(MwBotMessage, _cancellationTokenSource.Token);

        return true;
    }

    private async Task TimedAttemptReconnectAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient.IsConnected || _isConnecting)
            return;

        _logger.LogInformation("MwBot {id}: Attempting to reconnect to MQTT broker...", MwBot?.Id);
        var isConnected = await ConnectAsync();

        if (isConnected)
        {
            _logger.LogInformation("MwBot {id}: Reconnected to MQTT broker successfully.", MwBot?.Id);
            lock (_timerLock) if (!_timer.Enabled) _timer.Start();
        }
        else
        {
            await Task.Delay(5000, cancellationToken);
            await TimedAttemptReconnectAsync(cancellationToken);
        }
    }

    /// <summary>
    /// EVENT: What to do every _timer interval
    /// </summary>
    /// <param name="sender"></param>
    private async Task TimedProcessChargeRequest(CancellationToken cancellationToken)
    {
        try
        {
            if (MwBot is null)
            {
                _logger.LogWarning("MwBot not initialized, cannot process charge request");
                return;
            }

            if (!_mqttClient.IsConnected)
            {
                _logger.LogWarning("MwBot {id}: Disconnected. Attempting to reconnect...", MwBot.Id);
                await WaitForReconnection(cancellationToken);
            }

            if (MwBot.BatteryPercentage <= _rechargeThreshold || (MwBot.Status == MwBotStatus.Recharging || MwBot.Status == MwBotStatus.MovingToDock)) // Recharge if battery is low / resuming recharge
            {
                _logger.LogInformation("MwBot {id} battery is low: {batteryPercentage}%", MwBot.Id, MwBot.BatteryPercentage);
                lock (_timerLock) if (_timer.Enabled) _timer.Stop();
                _chargeRequested = 0;

                lock (_lockMessage)
                {
                    MwBotMessage.MessageType = MessageType.RequestRecharge;
                    MwBotMessage.Status = MwBotStatus.MovingToDock;
                }
            }
            else if (_chargeRequested >= _maxRequestsBeforeRecharge && MwBot.BatteryPercentage <= _maxRechargeThreshold) // If standing by for too long, request recharge
            {
                _logger.LogInformation("MwBot {id} has no charges to serve: requesting recharge", MwBot.Id);
                lock (_lockMessage) MwBotMessage.MessageType = MessageType.RequestRecharge;
                _chargeRequested = 0;
            }
            else // If nothing to do, request charge
            {
                lock (_lockMessage) MwBotMessage.MessageType = MessageType.RequestCharge;
                _chargeRequested++;
            }

            await PublishClientMessageAsync(MwBotMessage, _cancellationTokenSource.Token);
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

        switch(status)
        {
            case MwBotStatus.MovingToSlot:
                _logger.LogInformation("MwBot {id}: Simulating movement to car {delay} seconds...", MwBot.Id, delay);
                break;
            case MwBotStatus.MovingToDock:
                _logger.LogInformation("MwBot {id}: Simulating movement to dock {delay} seconds...", MwBot.Id, delay);
                break;
            default:
                _logger.LogWarning("MwBot {id}: Invalid status", MwBot.Id);
                return;
        }

        var changeSuccess = await ChangeBotStatus(status);
        if (!changeSuccess)
            return;

        await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);
    }

    private async Task SimulateChargingProcess(MqttClientMessage mwBotMessage, CancellationToken cancellationToken)
    {
        _logger.LogInformation("MwBot {id}: Starting SimulateChargingProcess.", MwBot?.Id);

        decimal chargeRate = 0.3m; // kW/s
        decimal dischargeRate = 0.5m; // kW/s
        decimal energyCostPerKw = mwBotMessage.Parking?.EnergyCostPerKw ?? 0.2m;
        decimal stopCostPerMinute = mwBotMessage.Parking?.StopCostPerMinute ?? 0.1m;

        var changeSuccess = await ChangeBotStatus(MwBotStatus.ChargingCar);
        if (!changeSuccess)
            return;

        var currentlyCharging = mwBotMessage.CurrentlyCharging;
        mwBotMessage.MessageType = MessageType.UpdateCharging;
        mwBotMessage.LatestLocation = MwBotLocations.InSlot;

        if (currentlyCharging != null && currentlyCharging.StartChargePercentage == 0 && currentlyCharging.CurrentChargePercentage == 0)
        {
            // Set random start charge percentage
            Random random = new();
            decimal minValue = 1;
            decimal maxValue = mwBotMessage.TargetBatteryPercentage ?? 50;
            decimal randomStartCharge = random.Next((int)minValue, (int)maxValue);

            currentlyCharging.StartChargingTime ??= DateTime.Now;
            currentlyCharging.StartChargePercentage = currentlyCharging.CurrentChargePercentage = mwBotMessage.CurrentCarCharge = randomStartCharge;

            currentlyCharging.StartChargePercentage = Math.Round(currentlyCharging.StartChargePercentage ?? 0, 2);
            await PublishClientMessageAsync(mwBotMessage, _cancellationTokenSource.Token);

            _logger.LogInformation("MwBot {id}: Starting charge from {startingBattery}% to {targetBattery:F0}%", mwBotMessage.Id, randomStartCharge, currentlyCharging.TargetChargePercentage);
        }
        else
        {
            mwBotMessage.CurrentCarCharge = currentlyCharging?.CurrentChargePercentage;
            _logger.LogInformation("MwBot {id}: Resuming charge from {currentBattery}% to {targetBattery:F0}%", mwBotMessage.Id, currentlyCharging?.CurrentChargePercentage, currentlyCharging?.TargetChargePercentage);
        }

        while (mwBotMessage.CurrentCarCharge < mwBotMessage.TargetBatteryPercentage && MwBot.BatteryPercentage > _rechargeThreshold && _isCharging)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_mqttClient.IsConnected)
            {
                _logger.LogWarning("MwBot {id}: Disconnected during charging. Waiting for reconnection...", MwBot?.Id);
                await WaitForReconnection(cancellationToken);
            }

            await Task.Delay(_chargeDelay, cancellationToken); // Simulate one second

            mwBotMessage.CurrentCarCharge += chargeRate;
            MwBot.BatteryPercentage -= dischargeRate;

            if (mwBotMessage.CurrentCarCharge > mwBotMessage.TargetBatteryPercentage)
            {
                mwBotMessage.CurrentCarCharge = mwBotMessage.TargetBatteryPercentage;
            }

            currentlyCharging.EnergyConsumed += chargeRate / 3600; // Convert kW/s to kWh

            var elapsedMinutes = (DateTime.Now - currentlyCharging.StartChargingTime).Value.TotalMinutes;
            currentlyCharging.TotalCost = currentlyCharging.EnergyConsumed * energyCostPerKw + (decimal)elapsedMinutes * stopCostPerMinute;

            _logger.LogInformation("MwBot {botId}: Car charge: {carCharge}% / {requestedCharge}% | MwBot battery: {botBattery}% | Time: {elapsedTime:F2} min | Energy consumed: {energyConsumed:F7} kWh | Cost: {totalCost:F2} EUR",
                MwBot.Id, mwBotMessage.CurrentCarCharge, mwBotMessage.TargetBatteryPercentage, MwBot.BatteryPercentage, elapsedMinutes, currentlyCharging.EnergyConsumed, currentlyCharging.TotalCost);

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

        if (mwBotMessage.CurrentCarCharge >= mwBotMessage.TargetBatteryPercentage || !_isCharging)
        {
            _logger.LogInformation("MwBot {botId}: Car charged to {carCharge}%", MwBot.Id, mwBotMessage.CurrentCarCharge);
            await CompleteChargingProcess(mwBotMessage);
        }
        else
        {
            _logger.LogInformation("MwBot {botId}: Stopping charging process due to low battery", MwBot.Id);
        }

        lock (_timerLock) if (!_timer.Enabled) _timer.Start();
        _logger.LogInformation("MwBot {id}: Ending SimulateChargingProcess.", MwBot.Id);
    }

    private async Task SimulateRechargingProcessAsync(CancellationToken cancellationToken)
    {
        if (MwBot == null)
        {
            _logger.LogWarning("MwBot not initialized, cannot recharge");
            return;
        }

        _isRecharging = true;
        decimal rechargeRate = 2.0m; // kW/s

        lock (_lockMessage)
        {
            MwBotMessage.MessageType = MessageType.UpdateMwBot;
            MwBotMessage.LatestLocation = MwBotLocations.InDock;
            MwBotMessage.Status = MwBotStatus.Recharging;
        }

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

            lock (_lockMessage) MwBotMessage.BatteryPercentage = MwBot.BatteryPercentage;
            await PublishClientMessageAsync(MwBotMessage, cancellationToken);
            _logger.LogInformation("MwBot {id} recharging: {batteryPercentage}%", MwBot.Id, MwBot.BatteryPercentage);
        }

        _logger.LogInformation("MwBot {id} completed recharging", MwBot.Id);

        // Change status to StandBy after recharging
        await ChangeBotStatus(MwBotStatus.StandBy);
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
            ParkingSlotId = HandlingRequest?.ParkingSlotId,
            TargetBatteryPercentage = HandlingRequest?.RequestedChargeLevel,
            CurrentlyCharging = currentlyCharging,
            UserId = HandlingRequest?.UserId,
            ParkingId = MwBot.ParkingId,
            ImmediateRequestId = HandlingRequest?.Id,
            CarPlate = HandlingRequest?.CarPlate,
        };

        await PublishClientMessageAsync(mwBotMessage, _cancellationTokenSource.Token);
        lock (_lockMessage) HandlingRequest = null;
        _isCharging = false;

        _logger.LogInformation("MwBot {botId}: Completed charging car at parking slot {slotId}. Time: {elapsedTime} , Energy consumed: {energyConsumed:F7} kWh, Cost: {totalCost:F2} EUR",
            MwBot?.Id, mwBotMessage.ParkingSlotId, elapsedMinutes, currentlyCharging.EnergyConsumed, currentlyCharging.TotalCost);
    }

    public async Task PublishClientMessageAsync(MqttClientMessage mwBotMessage, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(mwBotMessage);
        var success = await PublishAsync(payload);

        if(!success)
        {
            _logger.LogError("MwBot {id}: Error publishing message {payload}", MwBot?.Id, payload);
        }
        //else
        //{
        //    _logger.LogDebug("MwBot {id}: Published message {payload}", MwBot?.Id, payload);
        //}
    }

    public async Task<bool> PublishAsync(string payload)
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
                _logger.LogWarning("MwBot {id}: Client is not connected, attempting to reconnect...", MwBot?.Id);
                var isConnected = await ConnectAsync();

                if (!isConnected)
                {
                    _logger.LogError("MwBot {id}: Unable to reconnect to MQTT broker.", MwBot?.Id);
                    return false;
                }
            }

            await _mqttClient.PublishAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MwBot {id}: Error publishing message {payload}", MwBot?.Id, payload);
            return false;
        }
    }

    public async Task<bool> ConnectAsync()
    {
        await _connectionSemaphore.WaitAsync();

        try
        {
            if (_isConnecting)
            {
                _logger.LogWarning("MwBot {id}: Already attempting to connect. Skipping duplicate connection attempt.", MwBot?.Id);
                return false;
            }

            _isConnecting = true;

            if (_mqttClient.IsConnected)
            {
                _logger.LogInformation("MwBot {id}: Already connected to MQTT server.", MwBot?.Id);
                return true;
            }

            _logger.LogInformation("MwBot {id}: Connecting to MQTT server...", MwBot?.Id);
            await _mqttClient.ConnectAsync(_options, _cancellationTokenSource.Token);
            await _mqttClient.SubscribeAsync($"mwbot{MwBot?.Id}");
            _logger.LogInformation("MwBot {id}: Connected to MQTT server", MwBot?.Id);

            lock (_timerLock)
            {
                if (_reconnectTimer.Enabled) _reconnectTimer.Stop();
                if (!_timer.Enabled) _timer.Start();
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("MwBot {id}: Connection attempt was canceled.", MwBot?.Id);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MwBot {id}: Error connecting to MQTT server", MwBot?.Id);
            return false;
        }
        finally
        {
            _isConnecting = false;
            _connectionSemaphore.Release();
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
            lock (_timerLock)
            {
                if (_timer.Enabled) _timer.Stop();
                if (_reconnectTimer.Enabled) _reconnectTimer.Stop();
            }

            _cancellationTokenSource.Cancel();

            lock (_lockMessage)
            {
                MwBotMessage.MessageType = MessageType.DisconnectClient;
                MwBotMessage.Status = MwBot.Status = MwBotStatus.Offline;
                MwBotMessage.BatteryPercentage = MwBot.BatteryPercentage;
            }

            await PublishClientMessageAsync(MwBotMessage, _cancellationTokenSource.Token);
            await _mqttClient.DisconnectAsync();
            _logger.LogInformation("MwBot {id}: Disconnected from MQTT server", MwBot.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MwBot {id}: Error disconnecting from MQTT server", MwBot.Id);
            return false;
        }
    }

    private async Task HandleClientDisconnectedAsync(MqttClientDisconnectedEventArgs e)
    {
        _logger.LogWarning("MwBot {id}: Disconnected from MQTT broker. Reason: {reason}", MwBot?.Id, e.ReasonString);

        lock (_timerLock) if (_timer.Enabled) _timer.Stop();

        if (!_isConnecting)
        {
            _logger.LogInformation("MwBot {id}: Starting reconnection attempts.", MwBot?.Id);
            await WaitForReconnection(_cancellationTokenSource.Token);
        }
        else
        {
            _logger.LogInformation("MwBot {id}: Reconnection already in progress.", MwBot?.Id);
        }
    }

    private async Task WaitForReconnection(CancellationToken cancellationToken)
    {
        while (!_mqttClient.IsConnected)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("MwBot {id}: Attempting to reconnect...", MwBot?.Id);
            var isConnected = await ConnectAsync();

            if (isConnected)
            {
                _logger.LogInformation("MwBot {id}: Reconnected successfully.", MwBot?.Id);
                return;
            }
            else
            {
                _logger.LogWarning("MwBot {id}: Reconnection attempt failed. Retrying in 5 seconds...", MwBot?.Id);
                await Task.Delay(5000, cancellationToken);
            }
        }
    }

    private static async Task ChangeLightState(string lightId, string requestBody)
    {
        var url = $"{lightsUrl}{lightId}/state";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        await httpClient.PutAsync(url, content);
    }

}