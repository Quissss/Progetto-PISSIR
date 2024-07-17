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
        var topic = arg.ApplicationMessage.Topic;
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

            switch (mwBotMessage.Status)
            {
                case MwBotStatus.MovingToSlot:
                    await HandleMovingToSlotMessageAsync(mwBotMessage, arg, mwBotRepository);
                    break;
                case MwBotStatus.ChargingCar:
                    await HandleChargingCarMessageAsync(mwBotMessage, arg, mwBotRepository);
                    break;
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

    private async Task HandleMovingToSlotMessageAsync(MqttClientMessage mwBotMessage, InterceptingPublishEventArgs arg, MwBotRepository mwBotRepository)
    {
        _logger.LogDebug("MqttBroker: MwBot {id} is moving to slot", mwBotMessage.Id);

        // Send confirmation message to MwBot
        var confirmMessage = new MqttClientMessage
        {
            Id = mwBotMessage.Id,
            Status = mwBotMessage.Status,
            BatteryPercentage = mwBotMessage.BatteryPercentage,
            ParkingSlotId = mwBotMessage.ParkingSlotId,
            TargetBatteryPercentage = mwBotMessage.TargetBatteryPercentage,
            UserId = mwBotMessage.UserId
        };

        var confirmPayload = JsonSerializer.Serialize(confirmMessage);
        var topic = arg.ApplicationMessage.Topic;
        var message = new MqttApplicationMessageBuilder()
            .WithPayload(confirmPayload)
            .WithTopic(topic)
            .Build();

        await _mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(message) { SenderClientId = "MqttServer",  });

        _logger.LogDebug("Confirmation sent to MwBot {id}", mwBotMessage.Id);
    }

    private async Task HandleChargingCarMessageAsync(MqttClientMessage mwBotMessage, InterceptingPublishEventArgs arg, MwBotRepository mwBotRepository)
    {
        _logger.LogDebug("MqttBroker: MwBot {id} is charging car", mwBotMessage.Id);

        // Set random start charge percentage
        Random random = new Random();
        decimal minValue = 1;
        decimal maxValue = mwBotMessage.TargetBatteryPercentage ?? 50;
        decimal randomStartCharge = random.Next((int)minValue, (int)maxValue);

        var currentlyCharging = new CurrentlyCharging
        {
            StartChargingTime = DateTime.Now,
            StartChargePercentage = randomStartCharge,
            TargetChargePercentage = mwBotMessage.TargetBatteryPercentage,
            MwBotId = mwBotMessage.Id,
            UserId = mwBotMessage.UserId,
            ParkingSlotId = mwBotMessage.ParkingSlotId,

        };
        await _currentlyChargingRepository.AddAsync(currentlyCharging);

        // Send confirmation message to MwBot
        var confirmMessage = new MqttClientMessage
        {
            Id = mwBotMessage.Id,
            Status = MwBotStatus.StandBy,
            BatteryPercentage = mwBotMessage.BatteryPercentage,
            ParkingSlotId = mwBotMessage.ParkingSlotId,
            TargetBatteryPercentage = mwBotMessage.TargetBatteryPercentage,
            UserId = mwBotMessage.UserId,
            CarPlate = mwBotMessage.CarPlate,
            CurrentCarCharge = randomStartCharge
        };

        var confirmPayload = JsonSerializer.Serialize(confirmMessage);
        var message = new MqttApplicationMessageBuilder()
            .WithPayload(confirmPayload)
            .WithTopic(arg.ApplicationMessage.Topic)
            .Build();

        await _mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(message) { SenderClientId = "MqttServer" });

        _logger.LogDebug("Confirmation sent to MwBot {id}", mwBotMessage.Id);
    }

    /// <summary>
    /// Handle enqueued or dropped messages sent by MwBot client
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private Task MqttServer_ApplicationMessageEnqueuedOrDroppedAsync(ApplicationMessageEnqueuedEventArgs arg)
    {
        _logger.LogDebug("MqttBroker MessageEnqueuedOrDropped: Message: {payload}", arg.ApplicationMessage.PayloadSegment);
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