using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Services.MQTT;

public class MqttClient
{
    private readonly MqttClientOptions _options;
    private readonly ILogger<MqttClient> _logger;
    private readonly IMqttClient _mqttClient;

    public MqttClient(ILogger<MqttClient> logger)
    {
        _options = new MqttClientOptionsBuilder()
            .WithClientId(Guid.NewGuid().ToString())
            .WithTcpServer("localhost")
            .WithCredentials("username", "password")
            .WithCleanSession()
            .Build();

        _logger = logger;
        _mqttClient = new MqttFactory().CreateMqttClient();
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.BeginScope("Connecting to MQTT server");
            await _mqttClient.ConnectAsync(_options, cancellationToken);
            _logger.LogInformation("Connected to MQTT server");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Connection to MQTT server cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to MQTT server");
        }
    }

    public async Task DisconnectAsync()
    {
        _logger.BeginScope("Disconnecting from MQTT server");
        await _mqttClient.DisconnectAsync();
        _logger.LogInformation("Disconnected from MQTT server");
    }

    public async Task PingServer()
    {
        _logger.BeginScope("Pinging MQTT server");
        await _mqttClient.PingAsync();
        _logger.LogInformation("Pinged MQTT server");
    }

    // TODO : Implement reconnect using timer
}
