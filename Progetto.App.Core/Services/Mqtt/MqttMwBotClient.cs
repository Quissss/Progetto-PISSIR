using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
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
        //_timer = new Timer(TimedProcessChargeRequest, null, Timeout.Infinite, 10000);
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
            var handleRequest = await _chargeManager.ServeNext(mwBot.Id);

            if (handleRequest is not null)
            {
                _logger.BeginScope("Processing request {request}", handleRequest);

                // Simulate mwbot movement
                var changeSuccess = await ChangeBotStatus(MwBotStatus.MovingToSlot);
                if (!changeSuccess)
                    return;

                int maxDelaySeconds = 30; // 30 max seconds
                int minDelaySeconds = 10; // 10 minimum seconds
                int delayRange = maxDelaySeconds - minDelaySeconds;
                int delay = minDelaySeconds + (int)(delayRange * (1 - mwBot.BatteryPercentage / 100));
                _logger.LogInformation("Simulating movement for {delay} seconds...", delay);
                await Task.Delay(TimeSpan.FromSeconds(delay));

                await ProcessRequest(handleRequest);
            }
        }
    }

    /// <summary>
    /// Process request by sending message to MQTTBroker
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task ProcessRequest(ImmediateRequest request)
    {
        if (mwBot is null)
        {
            _logger.LogWarning("MwBot not initialized, cannot process request");
            return;
        }

        mwBot.Status = MwBotStatus.ChargingCar;
        var mwBotMessage = new MqttClientMessage
        {
            Id = mwBot.Id,
            Status = mwBot.Status,
            BatteryPercentage = mwBot.BatteryPercentage,
            ParkingSlotId = request.ParkingSlotId,
            TargetBatteryPercentage = request.RequestedChargeLevel
        };

        await PublishClientMessageAsync(mwBotMessage);
        /* TODO : broker deve 
         * impostare ChargingCar nel backend
         * aggiungere il record in CurrentlyCharging
         * 
                var changeSuccess = await ChangeBotStatus(MwBotStatus.MovingToSlot);
                if (!changeSuccess)
                    return;
        */
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
            .WithClientId(Guid.NewGuid().ToString())
            .WithTcpServer(brokerAddress, 1883)
            .WithTimeout(TimeSpan.FromSeconds(10))
            .WithCleanSession()
            .Build();

        await InitializeMwBot(mwBotId);
        var isConnected = await ConnectAsync();
        //if (isConnected)
        //{
        //    _timer.Change(0, 10000); // Initialize timer at 10 seconds
        //}
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

    /// <summary>
    /// Publish message to MQTT server/broker
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    public async Task PublishAsync(string payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var message = new MqttApplicationMessageBuilder()
            .WithPayload(payload)
            .Build();

        if (!_mqttClient.IsConnected)
        {
            _logger.LogWarning("Client is not connected, cannot publish message");
            return;
        }

        await _mqttClient.PublishAsync(message);
        _logger.LogInformation("Published message {payload}", payload);
    }

    /// <summary>
    /// Publish MwBot message to MQTT server/broker
    /// </summary>
    /// <param name="mwBotMessage"></param>
    /// <returns></returns>
    public async Task PublishClientMessageAsync(MqttClientMessage mwBotMessage)
    {
        var payload = JsonSerializer.Serialize(mwBotMessage);
        await PublishAsync(payload);
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
            _logger.LogInformation("MwBot updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating MwBot");
        }
    }

    /// <summary>
    /// Connect to MQTT server
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Disconnect from MQTT server
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Ping MQTT server
    /// </summary>
    /// <returns></returns>
    public async Task<bool> PingServer()
    {
        try
        {
            _logger.BeginScope("Pinging MQTT server");
            await _mqttClient.PingAsync();
            _logger.LogInformation("Ping sent to MQTT server");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending ping to MQTT server");
            return false;
        }
    }

    /// <summary>
    /// Subscribe to given topic
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    public async Task<bool> SubscribeAsync(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            _logger.LogWarning("Invalid topic, cannot subscribe");
            return false;
        }

        if (!_mqttClient.IsConnected)
        {
            _logger.LogWarning("Client is not connected, cannot subscribe to topic");
            return false;
        }

        try
        {
            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
            _logger.LogInformation("Subscribed to topic: {topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to topic: {topic}", topic);
            return false;
        }

        return true;
    }
}
