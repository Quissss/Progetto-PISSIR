﻿using Microsoft.Extensions.DependencyInjection;
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
    private MqttClientMessage MwBotMessage { get; set; } = new();
    private ImmediateRequest? HandlingRequest { get; set; }
    public MwBot? MwBot { get; set; }

    public object LockMessage => _lockMessage;

    public MqttClientOptions? Options { get => _options; set => _options = value; }

    public IMqttClient MqttClient => _mqttClient;

    public ILogger<MqttMwBotClient> Logger => _logger;

    public IServiceScopeFactory ServiceScopeFactory => _serviceScopeFactory;

    public static int RechargeThreshold => _rechargeThreshold;

    private CancellationTokenSource _cancellationTokenSource = new();
    private CancellationToken _cancellationToken;

    public CancellationTokenSource CancellationTokenSource { get => _cancellationTokenSource; set => _cancellationTokenSource = value; }
    public CancellationToken CancellationToken { get => _cancellationToken; set => _cancellationToken = value; }

    public static int ChargeDelay => _chargeDelay;

    public static int RechargeDelay => _rechargeDelay;

    private readonly object _lockMessage = new();

    private MqttClientOptions? _options;

    private readonly IMqttClient _mqttClient;
    private readonly ILogger<MqttMwBotClient> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private const int _rechargeThreshold = 5; // Battery percentage threshold for recharge triggering

    private readonly Timer _timer = new(1000);
    private readonly Timer _reconnectTimer = new(5000);

    private bool _isConnecting = false;
    private bool _isCharging = false;
    private bool _isRecharging = false;

    public Timer Timer => _timer;
    public Timer ReconnectTimer => _reconnectTimer;

    private const int _chargeDelay = 1000;
    private const int _rechargeDelay = 1000;

    private readonly object _timerLock = new object();
    private readonly SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(1, 1);

    public MqttMwBotClient(ILogger<MqttMwBotClient> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        CancellationToken = CancellationTokenSource.Token;

        _mqttClient = new MqttFactory().CreateMqttClient();
        _mqttClient.ApplicationMessageReceivedAsync += async (e) => await HandleReceivedApplicationMessage(e, CancellationToken);
        _mqttClient.DisconnectedAsync += async (e) => await HandleClientDisconnectedAsync(e);

        _timer.Elapsed += async (sender, e) => await TimedProcessChargeRequest(CancellationToken);
        _reconnectTimer.Elapsed += async (sender, e) => await TimedAttemptReconnectAsync(CancellationToken);
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
            // Smaltisci le risorse gestite
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

        // Libera le risorse non gestite (se presenti)

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
            Logger.LogWarning("Invalid message received");
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

        lock (LockMessage)
        {
            MwBotMessage.Id = MwBot.Id = brokerMessage.Id;
            MwBotMessage.Status = MwBot.Status = brokerMessage.Status;
            MwBotMessage.BatteryPercentage = MwBot.BatteryPercentage = brokerMessage.BatteryPercentage;
            MwBotMessage.ParkingId = MwBot.ParkingId = brokerMessage.ParkingId;
            MwBotMessage.Parking = MwBot.Parking = brokerMessage.Parking;
        }

        Logger.LogDebug("MwBot {id}: Received message: {payload} from topic: {message.ApplicationMessage.Topic}", MwBot.Id, payload, topic);

        try
        {
            lock (_timerLock) if (Timer.Enabled) Timer.Stop();
            switch (brokerMessage.MessageType)
            {
                case MessageType.StartCharging:
                    Logger.LogInformation("MwBot {id}: Received MessageType.StartCharging", MwBot.Id);

                    if (_isCharging)
                    {
                        Logger.LogWarning("MwBot {id}: Charging process is already running. Ignoring StartCharging message.", MwBot.Id);
                        break;
                    }

                    if (brokerMessage.ImmediateRequest is null)
                    {
                        Logger.LogDebug("MwBot {id}: No immediate request found. Changing status to StandBy", MwBot.Id);
                        await ChangeBotStatus(MwBotStatus.StandBy);
                        break;
                    }

                    _isCharging = true;
                    lock (LockMessage) HandlingRequest = brokerMessage.ImmediateRequest;

                    if (brokerMessage.LatestLocation != MwBotLocations.InSlot)
                        await SimulateMovement(MwBotStatus.MovingToSlot, cancellationToken);

                    await SimulateChargingProcess(brokerMessage, cancellationToken);
                    break;

                case MessageType.StartRecharge:
                    Logger.LogInformation("MwBot {id}: Received MessageType.StartRecharge", MwBot.Id);

                    if (_isRecharging)
                    {
                        Logger.LogWarning("MwBot {id}: Recharging process is already running. Ignoring StartRecharge message.", MwBot.Id);
                        break;
                    }

                    await StartRechargingAsync(cancellationToken);
                    _isRecharging = false;
                    break;

                case MessageType.ChargeCompleted:
                    Logger.LogInformation("MwBot {id}: Received MessageType.ChargeCompleted", MwBot.Id);
                    break;

                case MessageType.ReturnMwBot:
                    Logger.LogInformation("MwBot {id}: Received MessageType.ReturnMwBot", MwBot.Id);
                    break;

                default:
                    Logger.LogWarning("MwBot {id}: Invalid message type {type}", MwBot.Id, brokerMessage.MessageType);
                    break;
            }

            lock (LockMessage)
            {
                MwBotMessage.ParkingSlotId = HandlingRequest?.ParkingSlotId;
                MwBotMessage.TargetBatteryPercentage = HandlingRequest?.RequestedChargeLevel;
                MwBotMessage.UserId = HandlingRequest?.UserId;
                MwBotMessage.CarPlate = HandlingRequest?.CarPlate;
                MwBotMessage.ImmediateRequestId = HandlingRequest?.Id;
            }

            lock (_timerLock) if (!Timer.Enabled) Timer.Start();
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Operazione annullata.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing received arg: {arg}", arg);
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
        Options = new MqttClientOptionsBuilder()
            .WithClientId($"mwbot{mwBotId}")
            .WithTcpServer(brokerAddress, 1883)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(120)) // for debug purposes
            .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
            .WithCleanSession()
            .Build();

        var isConnected = await ConnectAsync();
        if (isConnected)
        {
            await MqttClient.SubscribeAsync($"mwbot{mwBotId}");
            await InitializeMwBot(mwBotId);
            lock (_timerLock) if (!Timer.Enabled) Timer.Start();
            if (!ReconnectTimer.Enabled) ReconnectTimer.Start();
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
            Logger.LogWarning("MwBot ID is null");
            return;
        }

        lock (LockMessage)
        {
            MwBotMessage.MessageType = MessageType.RequestMwBot;
            MwBotMessage.Id = mwBotId.Value;
        }

        await PublishClientMessageAsync(MwBotMessage, CancellationTokenSource.Token);
    }

    private async Task<bool> ChangeBotStatus(MwBotStatus status)
    {
        if (MwBot is null)
        {
            Logger.LogWarning("MwBot not initialized, cannot change status");
            return false;
        }

        switch (status)
        {
            case MwBotStatus.Offline:
                Logger.LogInformation("MwBot {id}: Disconnecting from MQTT server", MwBot.Id);
                await DisconnectAsync();
                break;
            case MwBotStatus.StandBy:
                Logger.LogInformation("MwBot {id}: Awaiting for task", MwBot.Id);
                lock (_timerLock) if (!Timer.Enabled) Timer.Start();
                break;
            case MwBotStatus.ChargingCar:
                Logger.LogInformation("MwBot {id}: Charging car", MwBot.Id);
                break;
            case MwBotStatus.Recharging:
                Logger.LogInformation("MwBot {id}: Recharging", MwBot.Id);
                break;
            case MwBotStatus.MovingToSlot:
                Logger.LogInformation("MwBot {id}: Going to charge car on parking slot {parkingSlot}", MwBot.Id, HandlingRequest?.ParkingSlotId);
                break;
            case MwBotStatus.MovingToDock:
                Logger.LogInformation("MwBot {id}: Going to dock for recharge", MwBot.Id);
                break;
            default:
                Logger.LogWarning("MwBot {id}: Invalid status", MwBot.Id);
                return false;
        }

        lock (LockMessage)
        {
            MwBotMessage.MessageType = MessageType.UpdateMwBot;
            MwBotMessage.Status = MwBot.Status = status;
        }

        await PublishClientMessageAsync(MwBotMessage, CancellationTokenSource.Token);

        return true;
    }

    private async Task TimedAttemptReconnectAsync(CancellationToken cancellationToken)
    {
        if (MqttClient.IsConnected || _isConnecting)
            return;

        Logger.LogInformation("MwBot {id}: Attempting to reconnect to MQTT broker...", MwBot?.Id);
        var isConnected = await ConnectAsync();

        if (isConnected)
        {
            Logger.LogInformation("MwBot {id}: Reconnected to MQTT broker successfully.", MwBot?.Id);
            lock (_timerLock) if (!Timer.Enabled) Timer.Start();
        }
        else
        {
            await Task.Delay(5000, cancellationToken);
            await TimedAttemptReconnectAsync(cancellationToken);
        }
    }

    public async Task StartRechargingAsync(CancellationToken cancellationToken)
    {
        if (MwBot == null)
        {
            Logger.LogWarning("MwBot not initialized, cannot start recharging");
            return;
        }

        if (MwBotMessage.LatestLocation != MwBotLocations.InDock)
            await SimulateMovement(MwBotStatus.MovingToDock, cancellationToken);
        await SimulateRechargingProcessAsync(cancellationToken);
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
                Logger.LogWarning("MwBot not initialized, cannot process charge request");
                return;
            }

            if (!MqttClient.IsConnected)
            {
                Logger.LogWarning("MwBot {id}: Disconnected. Attempting to reconnect...", MwBot.Id);
                await WaitForReconnection(cancellationToken);
            }

            if (MwBot.BatteryPercentage <= RechargeThreshold || (MwBot.Status == MwBotStatus.Recharging || MwBot.Status == MwBotStatus.MovingToDock)) // Recharge if battery is low / resuming recharge
            {
                Logger.LogInformation("MwBot {id} battery is low: {batteryPercentage}%", MwBot.Id, MwBot.BatteryPercentage);
                lock (_timerLock) if (Timer.Enabled) Timer.Stop();

                lock (LockMessage)
                {
                    MwBotMessage.MessageType = MessageType.RequestRecharge;
                    MwBotMessage.Status = MwBotStatus.MovingToDock;
                }
            }
            else // If nothing to do, request charge
            {
                lock (LockMessage)
                {
                    MwBotMessage.MessageType = MessageType.RequestCharge;
                }
            }

            await PublishClientMessageAsync(MwBotMessage, CancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing charge request");
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
            Logger.LogInformation("MwBot {id}: Simulating movement to car {delay} seconds...", MwBot.Id, delay);
        }
        else if (status == MwBotStatus.MovingToDock)
        {
            Logger.LogInformation("MwBot {id}: Simulating movement to dock {delay} seconds...", MwBot.Id, delay);
        }
        else
        {
            Logger.LogWarning("Invalid status");
            return;
        }

        var changeSuccess = await ChangeBotStatus(status);
        if (!changeSuccess)
            return;

        await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);
    }

    private async Task SimulateChargingProcess(MqttClientMessage mwBotMessage, CancellationToken cancellationToken)
    {
        Logger.LogInformation("MwBot {id}: Starting SimulateChargingProcess.", MwBot?.Id);

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

        if (currentlyCharging.StartChargePercentage == 0 && currentlyCharging.CurrentChargePercentage == 0)
        {
            // Set random start charge percentage
            Random random = new();
            decimal minValue = 1;
            decimal maxValue = mwBotMessage.TargetBatteryPercentage ?? 50;
            decimal randomStartCharge = random.Next((int)minValue, (int)maxValue);

            currentlyCharging.StartChargingTime ??= DateTime.Now;
            currentlyCharging.StartChargePercentage = currentlyCharging.CurrentChargePercentage = mwBotMessage.CurrentCarCharge = randomStartCharge;

            currentlyCharging.StartChargePercentage = Math.Round(currentlyCharging.StartChargePercentage ?? 0, 2);
            await PublishClientMessageAsync(mwBotMessage, CancellationTokenSource.Token);

            Logger.LogInformation("MwBot {id}: Starting charge from {startingBattery}% to {targetBattery:F0}%", mwBotMessage.Id, randomStartCharge, currentlyCharging.TargetChargePercentage);
        }
        else
        {
            mwBotMessage.CurrentCarCharge = currentlyCharging.CurrentChargePercentage;
            Logger.LogInformation("MwBot {id}: Resuming charge from {currentBattery}% to {targetBattery:F0}%", mwBotMessage.Id, currentlyCharging.CurrentChargePercentage, currentlyCharging.TargetChargePercentage);
        }

        while (mwBotMessage.CurrentCarCharge < mwBotMessage.TargetBatteryPercentage && MwBot.BatteryPercentage > RechargeThreshold)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!MqttClient.IsConnected)
            {
                Logger.LogWarning("MwBot {id}: Disconnected during charging. Waiting for reconnection...", MwBot?.Id);
                await WaitForReconnection(cancellationToken);
            }

            await Task.Delay(ChargeDelay, cancellationToken); // Simulate one second

            mwBotMessage.CurrentCarCharge += chargeRate;
            MwBot.BatteryPercentage -= dischargeRate;

            if (mwBotMessage.CurrentCarCharge > mwBotMessage.TargetBatteryPercentage)
            {
                mwBotMessage.CurrentCarCharge = mwBotMessage.TargetBatteryPercentage;
            }

            currentlyCharging.EnergyConsumed += chargeRate / 3600; // Convert kW/s to kWh

            var elapsedMinutes = (DateTime.Now - currentlyCharging.StartChargingTime).Value.TotalMinutes;
            currentlyCharging.TotalCost = currentlyCharging.EnergyConsumed * energyCostPerKw + (decimal)elapsedMinutes * stopCostPerMinute;

            Logger.LogInformation("MwBot {botId}: Car charge: {carCharge}% / {requestedCharge}% | MwBot battery: {botBattery}% | Time: {elapsedTime:F2} min | Energy consumed: {energyConsumed:F7} kWh | Cost: {totalCost:F2} EUR",
                MwBot.Id, mwBotMessage.CurrentCarCharge, mwBotMessage.TargetBatteryPercentage, MwBot.BatteryPercentage, elapsedMinutes, currentlyCharging.EnergyConsumed, currentlyCharging.TotalCost);

            currentlyCharging.CurrentChargePercentage = mwBotMessage.CurrentCarCharge;
            currentlyCharging.CurrentChargePercentage = Math.Round(currentlyCharging.CurrentChargePercentage.Value, 2);
            currentlyCharging.EnergyConsumed = Math.Round(currentlyCharging.EnergyConsumed, 7);
            currentlyCharging.TotalCost = Math.Round(currentlyCharging.TotalCost, 2);

            mwBotMessage.CurrentlyCharging = currentlyCharging;
            mwBotMessage.BatteryPercentage = MwBot.BatteryPercentage;

            await PublishClientMessageAsync(mwBotMessage, CancellationTokenSource.Token);
        }

        mwBotMessage.BatteryPercentage = MwBot.BatteryPercentage;
        await PublishClientMessageAsync(mwBotMessage, CancellationTokenSource.Token);

        if (mwBotMessage.CurrentCarCharge >= mwBotMessage.TargetBatteryPercentage)
        {
            Logger.LogInformation("MwBot {botId}: Car charged to {targetBattery}%", MwBot.Id, mwBotMessage.TargetBatteryPercentage);
            await CompleteChargingProcess(mwBotMessage);
            lock (_timerLock) if (!Timer.Enabled) Timer.Start();
        }
        else
        {
            Logger.LogInformation("MwBot {botId}: Stopping charging process due to low battery", MwBot.Id);
            _isCharging = false;
        }

        Logger.LogInformation("MwBot {id}: Ending SimulateChargingProcess.", MwBot.Id);
    }

    private async Task SimulateRechargingProcessAsync(CancellationToken cancellationToken)
    {
        if (MwBot == null)
        {
            Logger.LogWarning("MwBot not initialized, cannot recharge");
            return;
        }

        _isRecharging = true;
        decimal rechargeRate = 2.0m; // kW/s

        lock (LockMessage)
        {
            MwBotMessage.MessageType = MessageType.UpdateMwBot;
            MwBotMessage.LatestLocation = MwBotLocations.InDock;
            MwBotMessage.Status = MwBotStatus.Recharging;
        }

        while (MwBot.BatteryPercentage < 100)
        {
            await Task.Delay(RechargeDelay, cancellationToken); // Simulate one second

            if (cancellationToken.IsCancellationRequested)
            {
                Logger.LogInformation("Recharging process cancelled for MwBot {id}", MwBot.Id);
                return;
            }

            MwBot.BatteryPercentage += rechargeRate;
            if (MwBot.BatteryPercentage > 100)
            {
                MwBot.BatteryPercentage = 100;
            }

            lock (LockMessage)
            {
                MwBotMessage.BatteryPercentage = MwBot.BatteryPercentage;
            }

            await PublishClientMessageAsync(MwBotMessage, cancellationToken);

            Logger.LogInformation("MwBot {id} recharging: {batteryPercentage}%", MwBot.Id, MwBot.BatteryPercentage);
        }

        Logger.LogInformation("MwBot {id} completed recharging", MwBot.Id);

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

        await PublishClientMessageAsync(mwBotMessage, CancellationTokenSource.Token);
        lock (LockMessage) HandlingRequest = null;
        _isCharging = false;

        Logger.LogInformation("MwBot {botId}: Completed charging car at parking slot {slotId}. Time: {elapsedTime} , Energy consumed: {energyConsumed:F7} kWh, Cost: {totalCost:F2} EUR",
            MwBot?.Id, mwBotMessage.ParkingSlotId, elapsedMinutes, currentlyCharging.EnergyConsumed, currentlyCharging.TotalCost);
    }

    public async Task PublishClientMessageAsync(MqttClientMessage mwBotMessage, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(mwBotMessage);
        var success = await PublishAsync(payload);

        if(!success)
        {
            Logger.LogError("MwBot {id}: Error publishing message {payload}", MwBot?.Id, payload);
        }
        //else
        //{
        //    Logger.LogDebug("MwBot {id}: Published message {payload}", MwBot?.Id, payload);
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

            if (!MqttClient.IsConnected)
            {
                Logger.LogWarning("MwBot {id}: Client is not connected, attempting to reconnect...", MwBot?.Id);
                var isConnected = await ConnectAsync();

                if (!isConnected)
                {
                    Logger.LogError("MwBot {id}: Unable to reconnect to MQTT broker.", MwBot?.Id);
                    return false;
                }
            }

            await MqttClient.PublishAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "MwBot {id}: Error publishing message {payload}", MwBot?.Id, payload);
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
                Logger.LogWarning("MwBot {id}: Already attempting to connect. Skipping duplicate connection attempt.", MwBot?.Id);
                return false;
            }

            _isConnecting = true;

            if (MqttClient.IsConnected)
            {
                Logger.LogInformation("MwBot {id}: Already connected to MQTT server.", MwBot?.Id);
                return true;
            }

            Logger.LogInformation("MwBot {id}: Connecting to MQTT server...", MwBot?.Id);
            await MqttClient.ConnectAsync(Options, _cancellationToken);
            await MqttClient.SubscribeAsync($"mwbot{MwBot?.Id}");
            Logger.LogInformation("MwBot {id}: Connected to MQTT server", MwBot?.Id);

            lock (_timerLock)
            {
                if (ReconnectTimer.Enabled) ReconnectTimer.Stop();
                if (!Timer.Enabled) Timer.Start();
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning("MwBot {id}: Connection attempt was canceled.", MwBot?.Id);
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "MwBot {id}: Error connecting to MQTT server", MwBot?.Id);
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
            Logger.LogWarning("MwBot not initialized, cannot disconnect");
            return false;
        }

        if (MwBot.Status == MwBotStatus.Offline)
        {
            Logger.LogWarning("MwBot {id} is not online, cannot disconnect", MwBot.Id);
            return false;
        }

        try
        {
            Logger.BeginScope("MwBot {id}: Disconnecting from MQTT server", MwBot.Id);
            lock (_timerLock)
            {
                if (Timer.Enabled) Timer.Stop();
                if (ReconnectTimer.Enabled) ReconnectTimer.Stop();
            }

            CancellationTokenSource.Cancel();

            lock (LockMessage)
            {
                MwBotMessage.MessageType = MessageType.DisconnectClient;
                MwBotMessage.Status = MwBot.Status = MwBotStatus.Offline;
            }

            await PublishClientMessageAsync(MwBotMessage, CancellationTokenSource.Token);
            await MqttClient.DisconnectAsync();
            Logger.LogInformation("MwBot {id}: Disconnected from MQTT server", MwBot.Id);

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "MwBot {id}: Error disconnecting from MQTT server", MwBot.Id);
            return false;
        }
    }

    private async Task HandleClientDisconnectedAsync(MqttClientDisconnectedEventArgs e)
    {
        Logger.LogWarning("MwBot {id}: Disconnected from MQTT broker. Reason: {reason}", MwBot?.Id, e.ReasonString);

        lock (_timerLock) if (Timer.Enabled) Timer.Stop();

        if (!_isConnecting)
        {
            Logger.LogInformation("MwBot {id}: Starting reconnection attempts.", MwBot?.Id);
            await WaitForReconnection(CancellationToken);
        }
        else
        {
            Logger.LogInformation("MwBot {id}: Reconnection already in progress.", MwBot?.Id);
        }
    }

    private async Task WaitForReconnection(CancellationToken cancellationToken)
    {
        while (!MqttClient.IsConnected)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Logger.LogInformation("MwBot {id}: Attempting to reconnect...", MwBot?.Id);
            var isConnected = await ConnectAsync();

            if (isConnected)
            {
                Logger.LogInformation("MwBot {id}: Reconnected successfully.", MwBot?.Id);
                return;
            }
            else
            {
                Logger.LogWarning("MwBot {id}: Reconnection attempt failed. Retrying in 5 seconds...", MwBot?.Id);
                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}