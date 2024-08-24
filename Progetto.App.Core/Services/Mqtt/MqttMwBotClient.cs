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
public class MqttMwBotClient
{
    private MqttClientOptions? _options;
    private readonly IMqttClient _mqttClient;
    private readonly ILogger<MqttMwBotClient> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private const int _rechargeThreshold = 5; // Battery percentage threshold for recharging
    private Timer _timer;
    private Timer _reconnectTimer;
    private CancellationTokenSource _cancellationTokenSource;
    private CancellationToken _cancellationToken;
    private const int _chargeDelay = 1000;
    private const int _rechargeDelay = 1000;
    private MqttClientMessage _mwBotMessage { get; set; }
    private ImmediateRequest? HandlingRequest { get; set; }
    public MwBot? MwBot { get; set; }

    public MqttMwBotClient(ILogger<MqttMwBotClient> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;

        _mqttClient = new MqttFactory().CreateMqttClient();
        _mqttClient.ApplicationMessageReceivedAsync += async (e) => await HandleReceivedApplicationMessage(e, _cancellationToken);

        // Set timer for processing charge requests
        _timer = new Timer(1000);
        _timer.Elapsed += async (sender, e) => await TimedProcessChargeRequest(sender, _cancellationToken);

        // Set timer for reconnect attempts
        _reconnectTimer = new Timer(5000);
        _reconnectTimer.Elapsed += async (sender, e) => await TimedAttemptReconnectAsync(_cancellationToken);

        _mwBotMessage = new MqttClientMessage();
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

        _mwBotMessage.Id = MwBot.Id = brokerMessage.Id;
        _mwBotMessage.Status = MwBot.Status = brokerMessage.Status;
        _mwBotMessage.BatteryPercentage = MwBot.BatteryPercentage = brokerMessage.BatteryPercentage;
        _mwBotMessage.ParkingId = MwBot.ParkingId = brokerMessage.ParkingId;
        _mwBotMessage.Parking = MwBot.Parking = brokerMessage.Parking;

        _logger.LogDebug("MwBot {id}: Received message: {payload} from topic: {message.ApplicationMessage.Topic}", MwBot.Id, payload, topic);

        try
        {
            if (_timer.Enabled) _timer.Stop();
            switch (brokerMessage.MessageType)
            {
                case MessageType.StartCharging:
                    _logger.LogInformation("MwBot {id}: Received MessageType.StartCharging", MwBot.Id);
                    if (brokerMessage.ImmediateRequest is null)
                    {
                        await ChangeBotStatus(MwBotStatus.StandBy);
                        break;
                    }

                    HandlingRequest = brokerMessage.ImmediateRequest;
                    await SimulateMovement(MwBotStatus.MovingToSlot, cancellationToken);
                    await SimulateChargingProcess(brokerMessage, cancellationToken);
                    break;

                case MessageType.StartRecharge:
                    _logger.LogInformation("MwBot {id}: Received MessageType.StartRecharge", MwBot.Id);
                    await StartRechargingAsync(cancellationToken);
                    break;

                case MessageType.ChargeCompleted:
                    _logger.LogInformation("MwBot {id}: Received MessageType.ChargeCompleted", MwBot.Id);
                    break;

                case MessageType.ReturnMwBot:
                    _logger.LogInformation("MwBot {id}: Received MessageType.ReturnMwBot", MwBot.Id);
                    break;

                default:
                    _logger.LogWarning("MwBot {id}: Invalid message type {type}", MwBot.Id, brokerMessage.MessageType);
                    break;
            }

            _mwBotMessage.ParkingSlotId = HandlingRequest?.ParkingSlotId;
            _mwBotMessage.TargetBatteryPercentage = HandlingRequest?.RequestedChargeLevel;
            _mwBotMessage.UserId = HandlingRequest?.UserId;
            _mwBotMessage.CarPlate = HandlingRequest?.CarPlate;
            _mwBotMessage.ImmediateRequestId = HandlingRequest?.Id;

            if (!_timer.Enabled) _timer.Start();
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
            if (!_timer.Enabled) _timer.Start();
            if (!_reconnectTimer.Enabled) _reconnectTimer.Start();
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

        _mwBotMessage.MessageType = MessageType.RequestMwBot;
        _mwBotMessage.Id = mwBotId.Value;

        await PublishClientMessageAsync(_mwBotMessage, _cancellationTokenSource.Token);
    }

    private void ResetCancellationToken()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
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

        _mwBotMessage.MessageType = MessageType.UpdateMwBot;
        _mwBotMessage.Status = MwBot.Status = status;

        await PublishClientMessageAsync(_mwBotMessage, _cancellationTokenSource.Token);

        return true;
    }

    private async Task RequestResumeChargingAsync()
    {
        if (MwBot == null)
        {
            _logger.LogWarning("MwBot not initialized, cannot request to resume charging");
            return;
        }

        _mwBotMessage.MessageType = MessageType.RequestCharge;
        await PublishClientMessageAsync(_mwBotMessage, _cancellationTokenSource.Token);
    }

    private async Task TimedAttemptReconnectAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient.IsConnected)
            return;

        _logger.LogInformation("MwBot {id}: Attempting to reconnect to MQTT broker...", MwBot?.Id);
        var isConnected = await ConnectAsync();
        if (isConnected)
        {
            _logger.LogInformation("MwBot {id}: Reconnected to MQTT broker successfully.", MwBot?.Id);
            if (!_timer.Enabled) _timer.Start();
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
            _logger.LogWarning("MwBot not initialized, cannot start recharging");
            return;
        }

        await SimulateMovement(MwBotStatus.MovingToDock, cancellationToken);
        await SimulateRechargingProcessAsync(cancellationToken);
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

            if (MwBot.BatteryPercentage <= _rechargeThreshold || (MwBot.Status == MwBotStatus.Recharging || MwBot.Status == MwBotStatus.MovingToDock)) // Recharge if battery is low / resuming recharge
            {
                _logger.LogInformation("MwBot {id} battery is low: {batteryPercentage}%", MwBot.Id, MwBot.BatteryPercentage);
                if (_timer.Enabled) _timer.Stop();

                _mwBotMessage.MessageType = MessageType.RequestRecharge;
                _mwBotMessage.Status = MwBotStatus.MovingToDock;
            }
            else // If nothing to do, request charge
            {
                _mwBotMessage.MessageType = MessageType.RequestCharge;
            }

            await PublishClientMessageAsync(_mwBotMessage, _cancellationTokenSource.Token);
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

    private async Task SimulateChargingProcess(MqttClientMessage mwBotMessage, CancellationToken cancellationToken)
    {
        decimal chargeRate = 0.3m; // kW/s
        decimal dischargeRate = 0.5m; // kW/s
        decimal energyCostPerKw = mwBotMessage.Parking?.EnergyCostPerKw ?? 0.2m;
        decimal stopCostPerMinute = mwBotMessage.Parking?.StopCostPerMinute ?? 0.1m;

        var changeSuccess = await ChangeBotStatus(MwBotStatus.ChargingCar);
        if (!changeSuccess)
            return;

        var currentlyCharging = mwBotMessage.CurrentlyCharging;
        mwBotMessage.MessageType = MessageType.UpdateCharging;

        if (currentlyCharging.StartChargePercentage == 0 && currentlyCharging.CurrentChargePercentage == 0)
        {
            // Set random start charge percentage
            Random random = new Random();
            decimal minValue = 1;
            decimal maxValue = mwBotMessage.TargetBatteryPercentage ?? 50;
            decimal randomStartCharge = random.Next((int)minValue, (int)maxValue);

            if (currentlyCharging.StartChargingTime is null)
                currentlyCharging.StartChargingTime = DateTime.Now;
            currentlyCharging.StartChargePercentage = currentlyCharging.CurrentChargePercentage = mwBotMessage.CurrentCarCharge = randomStartCharge;

            currentlyCharging.StartChargePercentage = Math.Round(currentlyCharging.StartChargePercentage ?? 0, 2);
            await PublishClientMessageAsync(mwBotMessage, _cancellationTokenSource.Token);

            _logger.LogInformation("MwBot {id}: Starting charge from {startingBattery}% to {targetBattery:F0}%", mwBotMessage.Id, randomStartCharge, currentlyCharging.TargetChargePercentage);
        }
        else
        {
            mwBotMessage.CurrentCarCharge = currentlyCharging.CurrentChargePercentage;
            _logger.LogInformation("MwBot {id}: Resuming charge from {currentBattery}% to {targetBattery:F0}%", mwBotMessage.Id, currentlyCharging.CurrentChargePercentage, currentlyCharging.TargetChargePercentage);
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

            _mwBotMessage.MessageType = MessageType.UpdateMwBot;
            _mwBotMessage.Status = MwBotStatus.Recharging;
            _mwBotMessage.BatteryPercentage = MwBot.BatteryPercentage;

            await PublishClientMessageAsync(_mwBotMessage, cancellationToken);

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
            ParkingSlotId = HandlingRequest.ParkingSlotId,
            TargetBatteryPercentage = HandlingRequest.RequestedChargeLevel,
            CurrentlyCharging = currentlyCharging,
            UserId = HandlingRequest.UserId,
            ParkingId = MwBot.ParkingId,
            ImmediateRequestId = HandlingRequest.Id,
            CarPlate = HandlingRequest.CarPlate,
        };

        await PublishClientMessageAsync(mwBotMessage, _cancellationTokenSource.Token);
        HandlingRequest = null;

        _logger.LogInformation("MwBot {botId}: Completed charging car at parking slot {slotId}. Time: {elapsedTime} , Energy consumed: {energyConsumed:F7} kWh, Cost: {totalCost:F2} EUR",
            MwBot?.Id, mwBotMessage.ParkingSlotId, elapsedMinutes, currentlyCharging.EnergyConsumed, currentlyCharging.TotalCost);
    }

    public async Task PublishClientMessageAsync(MqttClientMessage mwBotMessage, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(mwBotMessage);
        await PublishAsync(payload);
        _logger.LogDebug("MwBot {id}: Published message {payload}", MwBot?.Id, payload);
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
            if (_reconnectTimer.Enabled) _reconnectTimer.Stop();
            ResetCancellationToken();

            _mwBotMessage.MessageType = MessageType.DisconnectClient;
            _mwBotMessage.Status = MwBot.Status = MwBotStatus.Offline;

            await PublishClientMessageAsync(_mwBotMessage, _cancellationTokenSource.Token);
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
