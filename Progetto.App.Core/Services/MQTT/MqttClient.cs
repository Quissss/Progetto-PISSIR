using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Serilog.Formatting.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
    private MwBot _mwBot;

    public MqttClient(ILogger<MqttClient> logger, MwBotRepository mwBotRepository, string webToken)
    {
        _options = new MqttClientOptionsBuilder()
            .WithClientId(Guid.NewGuid().ToString())
            .WithTcpServer("localhost")
            .WithCredentials("username", "password")
            .WithCleanSession()
            .Build();

        _logger = logger;
        _mwBotRepository = mwBotRepository;
        _mqttClient = new MqttFactory().CreateMqttClient();

        InitializeMwBot(webToken).Wait();
        _mqttClient.ApplicationMessageReceivedAsync += HandleReceivedApplicationMessage;
    }

    private async Task InitializeMwBot(string webToken)
    {
        _mwBot = await _mwBotRepository.SelectAsync(b => b.WebToken == webToken);

        if (_mwBot == null)
        {
            _mwBot = new MwBot
            {
                WebToken = webToken,
                BatteryPercentage = 100,
                Status = MwBotStatus.StandBy
            };
            await _mwBotRepository.AddAsync(_mwBot);
            await _mwBotRepository.SaveAsync();
            _logger.LogInformation($"Created new MwBot with WebToken: {webToken}");
        }
        else
        {
            _logger.LogInformation($"Found existing MwBot with WebToken: {webToken}");
        }
    }

    private async Task HandleReceivedApplicationMessage(MqttApplicationMessageReceivedEventArgs message)
    {
        if (message.ApplicationMessage.Payload == null)
        {
            _logger.LogWarning("Received message with no payload");
            return;
        }

        var payload = Encoding.UTF8.GetString(message.ApplicationMessage.Payload);
        _logger.LogInformation($"Received message: {payload} from topic: {message.ApplicationMessage.Topic}");

        var mwBotUpdate = JsonSerializer.Deserialize<MwBot>(payload);

        if (mwBotUpdate != null && mwBotUpdate.WebToken == _mwBot.WebToken)
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
            catch
        }
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
