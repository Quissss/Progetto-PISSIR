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
    private ChargeHistoryRepository _chargeHistoryRepository;

    public MqttBroker(ILogger<MqttBroker> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _options = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(1883) // TODO : Move to appsettings.json
            .Build();

        _logger = logger;
        _mqttServer = new MqttFactory().CreateMqttServer(_options);
        _serviceScopeFactory = serviceScopeFactory;

        var provider = _serviceScopeFactory.CreateScope().ServiceProvider;
        _currentlyChargingRepository = provider.GetRequiredService<CurrentlyChargingRepository>();
        _chargeHistoryRepository = provider.GetRequiredService<ChargeHistoryRepository>();

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
        if (arg.ClientId == "MqttServer")
            return;

        string payload = Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment);
        var mwBotMessage = JsonSerializer.Deserialize<MqttClientMessage>(payload);
        _logger.LogDebug("MqttBroker: MwBot message: {mwBotMessage}", mwBotMessage);

        if (mwBotMessage != null)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mwBotRepository = scope.ServiceProvider.GetRequiredService<MwBotRepository>();
            _logger.BeginScope("MqttBroker: Handling message by {id}", mwBotMessage.Id);

            var mwBot = await mwBotRepository.GetByIdAsync(mwBotMessage.Id);

            if (mwBot is null)
            {
                _logger.LogDebug("MqttBroker: MwBot doesn't exist");
                return;
            }

            switch (mwBotMessage.MessageType)
            {
                case MessageType.RequestCharge:
                    await HandleChargeRequestMessageAsync(mwBotMessage, arg, mwBotRepository);
                    break;
                case MessageType.CompleteCharge:
                    await HandleCompletedChargeMessageAsync(mwBotMessage, arg, mwBotRepository);
                    break;
                default:
                    _logger.LogDebug("MqttBroker: Invalid topic {topic}", arg.ApplicationMessage.Topic);
                    break;
            }

            _logger.LogDebug("Updating mwBot with params: {battery} and {status}", mwBotMessage.BatteryPercentage, mwBot.Status);
            mwBot.Status = mwBotMessage.Status;
            mwBot.BatteryPercentage = mwBotMessage.BatteryPercentage;
            await mwBotRepository.UpdateAsync(mwBot);
        }

        await Task.CompletedTask;
    }

    private async Task HandleCompletedChargeMessageAsync(MqttClientMessage mwBotMessage, InterceptingPublishEventArgs arg, MwBotRepository mwBotRepository)
    {
        // TODO: Set charge to be payed and free parking slot
        throw new NotImplementedException();
    }

    private async Task HandleChargeRequestMessageAsync(MqttClientMessage mwBotMessage, InterceptingPublishEventArgs arg, MwBotRepository mwBotRepository)
    {
        _logger.LogDebug("MqttBroker: MwBot {id} is charging car", mwBotMessage.Id);

        // TODO: get user + car on parking slot
        var currentlyCharging = new CurrentlyCharging
        {
            TargetChargePercentage = mwBotMessage.TargetBatteryPercentage,
            MwBotId = mwBotMessage.Id,
            UserId = mwBotMessage.UserId,
            ParkingSlotId = mwBotMessage.ParkingSlotId,
        };
        await _currentlyChargingRepository.AddAsync(currentlyCharging);

        // Send confirmation message to MwBot
        var confirmMessage = new MqttClientMessage
        {
            MessageType = MessageType.StartCharging,
            Id = mwBotMessage.Id,
            Status = MwBotStatus.ChargingCar,
            BatteryPercentage = mwBotMessage.BatteryPercentage,
            ParkingSlotId = mwBotMessage.ParkingSlotId,
            TargetBatteryPercentage = mwBotMessage.TargetBatteryPercentage,
            UserId = mwBotMessage.UserId,
            CarPlate = mwBotMessage.CarPlate,
            CurrentlyCharging = currentlyCharging
        };

        var confirmPayload = JsonSerializer.Serialize(confirmMessage);
        var message = new MqttApplicationMessageBuilder()
            .WithPayload(confirmPayload)
            .WithTopic($"mwbot{mwBotMessage.Id}")
            .Build();

        await _mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(message) { SenderClientId = "MqttServer" });
        _logger.LogDebug("Created charging record + confirmation sent to MwBot {id}", mwBotMessage.Id);
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