using Microsoft.Extensions.DependencyInjection;
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

/// <summary>
/// MwBot's MQTT broker (server)
/// </summary>
public class MqttBroker : IHostedService, IDisposable
{
    private readonly MqttServer _mqttServer;
    private readonly MqttServerOptions _options;
    private readonly ILogger<MqttBroker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private CurrentlyChargingRepository _currentlyChargingRepository;

    public MqttBroker(ILogger<MqttBroker> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _options = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(1883) // TODO : Move to appsettings.json
            .Build();

        _logger = logger;
        _mqttServer = new MqttFactory().CreateMqttServer(_options);
        _serviceScopeFactory = serviceScopeFactory;

        using var scope = _serviceScopeFactory.CreateScope();
        _currentlyChargingRepository = scope.ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();

        _mqttServer.ApplicationMessageEnqueuedOrDroppedAsync += MqttServer_ApplicationMessageEnqueuedOrDroppedAsync;
        _mqttServer.ClientConnectedAsync += MqttServer_ClientConnectedAsync;
        _mqttServer.ClientDisconnectedAsync += MqttServer_ClientDisconnectedAsync;
        _mqttServer.InterceptingPublishAsync += MqttServer_InterceptingPublishAsync;
    }

    /// <summary>
    /// Intercept published messages sent by MwBot client
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private async Task MqttServer_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
    {
        string payload = Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment);
        var mwBotMessage = JsonSerializer.Deserialize<MqttClientMessage>(payload);
        _logger.LogDebug("MqttBroker: MwBot message: {mwBotMessage}", mwBotMessage);

        if (mwBotMessage != null)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mwBotRepository = scope.ServiceProvider.GetRequiredService<MwBotRepository>();
            _logger.BeginScope("MqttBroker: Handling message by {id}", mwBotMessage.Id);

            if (mwBotMessage.Status == MwBotStatus.ChargingCar)
            { // TODO: check currently charging, format correct message
                _logger.LogDebug("MqttBroker: MwBot {id} is charging car", mwBotMessage.Id);
                var currentlyCharging = new CurrentlyCharging
                {
                    StartChargingTime = DateTime.Now,
                    StartChargePercentage = mwBotMessage.CurrentCarCharge,
                    TargetChargePercentage = mwBotMessage.TargetBatteryPercentage,
                    MwBotId = mwBotMessage.Id,
                    UserId = mwBotMessage.UserId,
                    CarPlate = mwBotMessage.CarPlate,
                    ParkingSlotId = mwBotMessage.ParkingSlotId
                };
                await _currentlyChargingRepository.AddAsync(currentlyCharging);
            }

            var mwBot = await mwBotRepository.GetByIdAsync(mwBotMessage.Id);

            if (mwBot is null)
            {
                _logger.LogDebug("MqttBroker: MwBot doesn't exist");
                return;
            }

            _logger.LogDebug("Updating local mwBot with params: {battery} and {status}", mwBotMessage.BatteryPercentage, mwBot.Status);
            mwBot.Status = mwBotMessage.Status;
            mwBot.BatteryPercentage = mwBotMessage.BatteryPercentage;
            await mwBotRepository.UpdateAsync(mwBot);
        }

        arg.ProcessPublish = true;
    }

    /// <summary>
    /// Handle enqueued or dropped messages sent by MwBot client
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private Task MqttServer_ApplicationMessageEnqueuedOrDroppedAsync(ApplicationMessageEnqueuedEventArgs arg)
    {
        _logger.LogDebug("Message: {payload}", arg.ApplicationMessage.PayloadSegment);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Client disconnected event
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private Task MqttServer_ClientDisconnectedAsync(ClientDisconnectedEventArgs arg)
    {
        _logger.LogDebug("Client disconnected");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Client connected event
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private Task MqttServer_ClientConnectedAsync(ClientConnectedEventArgs arg)
    {
        _logger.LogDebug("Client connected");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Start MQTT server
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting MQTT server");
        await _mqttServer.StartAsync();
    }

    /// <summary>
    /// Stop MQTT server
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Stopping MQTT server");
        await _mqttServer.StopAsync();
    }

    /// <summary>
    /// Dispose MQTT server
    /// </summary>
    public void Dispose()
    {
        _logger.LogDebug("Disposing MQTT server");
        _mqttServer?.Dispose();
    }
}