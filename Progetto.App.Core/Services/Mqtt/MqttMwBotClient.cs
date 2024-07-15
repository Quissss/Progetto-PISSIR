using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
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
    private readonly IServiceScopeFactory _serviceScopeFactory; // Retrieve scoped services (repository in this case)
    private readonly ChargeManager _chargeManager;
    private Timer _timer;
    public MwBot? mwBot { get; private set; }

    public MqttMwBotClient(ILogger<MqttMwBotClient> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _chargeManager = new ChargeManager();
        _mqttClient = new MqttFactory().CreateMqttClient();
        _mqttClient.ApplicationMessageReceivedAsync += HandleReceivedApplicationMessage;

        // Configura il timer per eseguire ogni 10 secondi
        _timer = new Timer(OnTimedEvent, null, Timeout.Infinite, 1000);
    }

    /// <summary>
    /// What to do every _timer interval
    /// </summary>
    /// <param name="state"></param>
    private async void OnTimedEvent(object state)
    {
        if (mwBot?.Status == MwBotStatus.StandBy)
        {
            var result = _chargeManager.ServeNext();

            if (result is ImmediateRequest immediateRequest)
            {
                _logger.LogInformation($"Serving immediate request from user {immediateRequest.UserId} at {immediateRequest.RequestDate}.");
                await ProcessRequest(immediateRequest);
            }
            else if (result is Reservation reservation)
            {
                _logger.LogInformation($"Serving reservation from user {reservation.UserId} for {reservation.ReservationTime}.");
                await ProcessRequest(reservation);
            }
            else
            {
                _logger.LogInformation(result.ToString());
            }
        }
    }

    /// <summary>
    /// Process request by sending message to MQTTBroker
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task ProcessRequest(object request)
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
            BatteryPercentage = mwBot.BatteryPercentage
        };
        await PublishClientMessageAsync(mwBotMessage);
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
        if (isConnected)
        {
            _timer.Change(0, 10000); // Inizia il timer immediatamente, con un intervallo di 10 secondi
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
