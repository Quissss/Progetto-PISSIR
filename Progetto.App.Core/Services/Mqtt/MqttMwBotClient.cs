using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using Progetto.App.Core.Models;
using Progetto.App.Core.Models.Mqtt;
using Progetto.App.Core.Repositories;
using Serilog.Formatting.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Progetto.App.Core.Services.MQTT;

public class MqttMwBotClient
{
    private readonly ILogger<MqttMwBotClient> _logger;
    private readonly IMqttClient _mqttClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private MqttClientOptions _options;
    public MwBot MwBot { get; private set; }

    public MqttMwBotClient(ILogger<MqttMwBotClient> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _mqttClient = new MqttFactory().CreateMqttClient();
        _mqttClient.ApplicationMessageReceivedAsync += HandleReceivedApplicationMessage;
    }

    public async Task InitializeAsync(int? mwBotId, string brokerAddress = "localhost")
    {
        _options = new MqttClientOptionsBuilder()
            .WithClientId(Guid.NewGuid().ToString())
            .WithTcpServer(brokerAddress)
            .WithTlsOptions(new MqttClientTlsOptions { UseTls = true })
            .WithCleanSession()
            .Build();

        await InitializeMwBot(mwBotId);
        await ConnectAsync(CancellationToken.None);
    }

    private async Task InitializeMwBot(int? mwBotId)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mwBotRepository = scope.ServiceProvider.GetRequiredService<MwBotRepository>();

            if (mwBotId is not null)
                MwBot = await mwBotRepository.GetByIdAsync(mwBotId.Value);

            if (MwBot is null)
                _logger.LogWarning("MwBot not found");
            else
                _logger.LogInformation("MwBot initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while initializing MwBot with id: {mwBotId}", mwBotId);
        }
    }

    // Publish message to MQTT server
    public async Task PublishAsync(string topic, string payload)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .Build();

        if (_mqttClient.IsConnected)
        {
            await _mqttClient.PublishAsync(message);
            _logger.LogInformation($"Published message to topic: {topic}");
        }
        else
        {
            _logger.LogWarning("Client is not connected, cannot publish message");
        }
    }

    public async Task PublishClientMessageAsync(MqttClientMessage mwBotMessage)
    {
        var payload = JsonSerializer.Serialize(mwBotMessage);
        await PublishAsync("topic/mwbots", payload); // TODO : Paramtrize topic
    }

    // Handle received message
    private async Task HandleReceivedApplicationMessage(MqttApplicationMessageReceivedEventArgs message)
    {
        var payload = Encoding.UTF8.GetString(message.ApplicationMessage.Payload);
        _logger.LogInformation($"Received message: {payload} from topic: {message.ApplicationMessage.Topic}");

        var mwBotMessage = JsonSerializer.Deserialize<MqttClientMessage>(payload);

        if (mwBotMessage != null)
        {
            try
            {
                _logger.BeginScope("Updating MwBot");
                MwBot.Status = mwBotMessage.Status;
                MwBot.BatteryPercentage = mwBotMessage.BatteryPercentage;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var mwBotRepository = scope.ServiceProvider.GetRequiredService<MwBotRepository>();
                    await mwBotRepository.UpdateAsync(MwBot);
                }
                _logger.LogInformation("MwBot updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating MwBot");
            }
        }
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.BeginScope("Connecting to MQTT server");
            using var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            await _mqttClient.ConnectAsync(_options, timeoutToken.Token);
            _logger.LogInformation("Connected to MQTT server");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Timeout while connecting.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to MQTT server");
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            _logger.BeginScope("Disconnecting from MQTT server");
            MwBot.Status = MwBotStatus.Offline;

            using var scope = _serviceScopeFactory.CreateScope();
            var mwBotRepository = scope.ServiceProvider.GetRequiredService<MwBotRepository>();
            await mwBotRepository.UpdateAsync(MwBot);

            await _mqttClient.DisconnectAsync();
            _logger.LogInformation("Disconnected from MQTT server");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from MQTT server");
        }
    }

    public async Task PingServer()
    {
        try
        {
            _logger.BeginScope("Pinging MQTT server");
            await _mqttClient.PingAsync();
            _logger.LogInformation("Ping sent to MQTT server");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending ping to MQTT server");
        }
    }

    public async Task SubscribeAsync(string topic)
    {
        if (_mqttClient.IsConnected)
        {
            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
            _logger.LogInformation($"Subscribed to topic: {topic}");
        }
        else
        {
            _logger.LogWarning("Client is not connected, cannot subscribe to topic");
        }
    }
}
