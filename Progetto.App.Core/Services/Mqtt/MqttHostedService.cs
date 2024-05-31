using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Server;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Services.Mqtt;

public class MqttHostedService : IHostedService, IDisposable
{
    private readonly MqttServer _mqttServer;
    private readonly ILogger<MqttHostedService> _logger;

    public MqttHostedService(ILogger<MqttHostedService> logger)
    {
        var options = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(1883)
            .Build();

        _logger = logger;
        _mqttServer = new MqttFactory().CreateMqttServer(options);

        _mqttServer.ApplicationMessageEnqueuedOrDroppedAsync += MqttServer_ApplicationMessageEnqueuedOrDroppedAsync;
        _mqttServer.ClientConnectedAsync += MqttServer_ClientConnectedAsync;
        _mqttServer.ClientDisconnectedAsync += MqttServer_ClientDisconnectedAsync;
        _mqttServer.InterceptingPublishAsync += MqttServer_InterceptingPublishAsync;
    }

    private Task MqttServer_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
    {
        _logger.LogDebug("Message: {payload}", arg.ApplicationMessage.PayloadSegment);
        arg.ProcessPublish = true;
        return Task.CompletedTask;
    }

    private Task MqttServer_ClientDisconnectedAsync(ClientDisconnectedEventArgs arg)
    {
        _logger.LogDebug("Client disconnected");
        return Task.CompletedTask;
    }

    private Task MqttServer_ClientConnectedAsync(ClientConnectedEventArgs arg)
    {
        _logger.LogDebug("Client connected");
        return Task.CompletedTask;
    }

    private Task MqttServer_ApplicationMessageEnqueuedOrDroppedAsync(ApplicationMessageEnqueuedEventArgs arg)
    {
        _logger.LogDebug("Message: {payload}", arg.ApplicationMessage.PayloadSegment);
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _mqttServer.StartAsync();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _mqttServer.StopAsync();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _mqttServer.Dispose();
    }
}