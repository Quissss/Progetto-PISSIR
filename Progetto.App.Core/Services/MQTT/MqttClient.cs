using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using Progetto.App.Core.Models;
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

public class MqttClient
{
    private readonly MqttClientOptions _options;
    private readonly ILogger<MqttClient> _logger;
    private readonly IMqttClient _mqttClient;
    private readonly MwBotRepository _mwBotRepository;
    private MwBot? _mwBot;

    public MqttClient(ILogger<MqttClient> logger, MwBotRepository mwBotRepository, string topic)
    {
        _options = new MqttClientOptionsBuilder()
            .WithClientId(Guid.NewGuid().ToString())
            .WithTcpServer("localhost")
            .WithTlsOptions(new MqttClientTlsOptions
            {
                UseTls = true
            })
            .WithCleanSession()
            .Build();

        _logger = logger;
        _mwBotRepository = mwBotRepository;
        _mqttClient = new MqttFactory().CreateMqttClient();

        InitializeMwBot(topic).Wait();
        _mqttClient.ApplicationMessageReceivedAsync += HandleReceivedApplicationMessage;

        ConnectAsync(CancellationToken.None).Wait();
    }

    private async Task InitializeMwBot(string topic)
    {
        try
        {
            _mwBot = await _mwBotRepository.SelectAsync(b => b.MqttTopic == topic);

            if (_mwBot == null) // Create new MwBot if not found
            {
                _mwBot = new MwBot
                {
                    MqttTopic = topic,
                    BatteryPercentage = 100,
                    Status = MwBotStatus.StandBy
                };
                await _mwBotRepository.AddAsync(_mwBot);
                await _mwBotRepository.SaveAsync();
                _logger.LogInformation($"Created new MwBot with WebToken: {topic}");
            }
            else // Use existing MwBot
            {
                _logger.LogInformation($"Found existing MwBot with WebToken: {topic}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while initializing MwBot with WebToken: {webToken}", topic);
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

    // Handle received message
    private async Task HandleReceivedApplicationMessage(MqttApplicationMessageReceivedEventArgs message)
    {
        var payload = Encoding.UTF8.GetString(message.ApplicationMessage.Payload);
        _logger.LogInformation($"Received message: {payload} from topic: {message.ApplicationMessage.Topic}");

        var mwBotUpdate = JsonSerializer.Deserialize<MwBot>(payload);

        if (mwBotUpdate != null && mwBotUpdate.MqttTopic == _mwBot.MqttTopic)
        {
            try
            {
                _logger.BeginScope("Updating MwBot");
                _mwBot.Status = mwBotUpdate.Status;
                _mwBot.BatteryPercentage = mwBotUpdate.BatteryPercentage;

                await _mwBotRepository.UpdateAsync(_mwBot);
                await _mwBotRepository.SaveAsync();
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
            using (var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)))
            {
                await _mqttClient.ConnectAsync(_options, timeoutToken.Token);
            }
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

            // Set bot status to offline
            _mwBot.Status = MwBotStatus.Offline;
            await _mwBotRepository.UpdateAsync(_mwBot);
            await _mwBotRepository.SaveAsync();

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
}
