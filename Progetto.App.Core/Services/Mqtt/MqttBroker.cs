﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Server;
using Progetto.App.Core.Models;
using Progetto.App.Core.Models.Mqtt;
using Progetto.App.Core.Repositories;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Progetto.App.Core.Services.Mqtt;

public class MqttBroker : IHostedService, IDisposable
{
    private readonly MqttServer _mqttServer;
    private readonly ILogger<MqttBroker> _logger;
    private readonly MqttServerOptions _options;
    private readonly MwBotRepository _mwBotRepository;

    public MqttBroker(ILogger<MqttBroker> logger, MwBotRepository mwBotRepository)
    {
        _options = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(1883)
            .Build();

        _logger = logger;
        _mqttServer = new MqttFactory().CreateMqttServer(_options);
        _mwBotRepository = mwBotRepository;

        _mqttServer.ApplicationMessageEnqueuedOrDroppedAsync += MqttServer_ApplicationMessageEnqueuedOrDroppedAsync;
        _mqttServer.ClientConnectedAsync += MqttServer_ClientConnectedAsync;
        _mqttServer.ClientDisconnectedAsync += MqttServer_ClientDisconnectedAsync;
        _mqttServer.InterceptingPublishAsync += MqttServer_InterceptingPublishAsync;
    }

    private async Task MqttServer_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
    {
        _logger.LogDebug("Message: {payload}", arg.ApplicationMessage.PayloadSegment);

        string payload = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
        var mwBotMessage = JsonSerializer.Deserialize<MqttClientMessage>(payload);

        if (mwBotMessage != null)
        {
            var mwBot = await _mwBotRepository.GetByIdAsync(mwBotMessage.Id);

            if (mwBot != null)
            {
                mwBot.Status = mwBotMessage.Status;
                mwBot.BatteryPercentage = mwBotMessage.BatteryPercentage;

                await _mwBotRepository.UpdateAsync(mwBot);
            }
        }

        arg.ProcessPublish = true;
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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _mqttServer.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _mqttServer.StopAsync();
    }

    public void Dispose()
    {
        _mqttServer?.Dispose();
    }
}