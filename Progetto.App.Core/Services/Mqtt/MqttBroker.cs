using Microsoft.Extensions.Configuration;
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
using static System.Formats.Asn1.AsnWriter;

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

    public MqttBroker(ILogger<MqttBroker> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        _options = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(configuration.GetValue<int>("MqttPort"))
            .WithKeepAlive()
            .Build();

        _logger = logger;
        _mqttServer = new MqttFactory().CreateMqttServer(_options);
        _serviceScopeFactory = serviceScopeFactory;

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
                case MessageType.RequestResumeCharging:
                    await HandleResumeChargingRequestMessageAsync(mwBotMessage, mwBotRepository);
                    break;
                case MessageType.RequestCharge:
                    await HandleChargeRequestMessageAsync(mwBotMessage, arg, mwBotRepository);
                    break;
                case MessageType.CompleteCharge:
                    await HandleCompletedChargeMessageAsync(mwBotMessage, arg, mwBotRepository);
                    break;
                case MessageType.RequestRecharge:
                    await HandleRechargeRequestMessageAsync(mwBotMessage, arg, mwBotRepository);
                    break;
                case MessageType.UpdateCharging:
                    var currentlyChargingRepository = scope.ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();
                    await currentlyChargingRepository.UpdateAsync(mwBotMessage.CurrentlyCharging);
                    break;
                case MessageType.UpdateMwBot:
                    mwBot.Status = mwBotMessage.Status;
                    mwBot.BatteryPercentage = mwBotMessage.BatteryPercentage;
                    await mwBotRepository.UpdateAsync(mwBot);
                    break;
                case MessageType.UpdateParkingSlot:
                    var parkingSlotRepository = scope.ServiceProvider.GetRequiredService<ParkingSlotRepository>();
                    var parkingSlot = await parkingSlotRepository.GetByIdAsync(mwBotMessage.ParkingSlotId.Value);
                    parkingSlot.Status = mwBotMessage.ParkingSlot.Status;
                    await parkingSlotRepository.UpdateAsync(parkingSlot);
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

    private async Task HandleRechargeRequestMessageAsync(MqttClientMessage mwBotMessage, InterceptingPublishEventArgs arg, MwBotRepository mwBotRepository)
    {
        _logger.LogDebug("MqttBroker: MwBot {id} is requesting recharge", mwBotMessage.Id);

        var mwBot = await mwBotRepository.GetByIdAsync(mwBotMessage.Id);
        if (mwBot == null)
        {
            _logger.LogWarning("MwBot not found");
            return;
        }

        mwBot.Status = MwBotStatus.MovingToDock;
        await mwBotRepository.UpdateAsync(mwBot);

        var confirmMessage = mwBotMessage;
        confirmMessage.MessageType = MessageType.StartRecharge;
        confirmMessage.Id = mwBot.Id;
        confirmMessage.Status = mwBot.Status;
        confirmMessage.BatteryPercentage = mwBot.BatteryPercentage;

        await PublishMessage(confirmMessage);
        _logger.LogDebug("Confirmation sent to MwBot {id} to start recharging", mwBotMessage.Id);
    }

    private async Task HandleResumeChargingRequestMessageAsync(MqttClientMessage mwBotMessage, MwBotRepository mwBotRepository)
    {
        _logger.LogDebug("MqttBroker: MwBot {id} requested to resume charging", mwBotMessage.Id);

        var scope = _serviceScopeFactory.CreateScope();
        var currentlyChargingRepository = scope.ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();


        var responseMessage = mwBotMessage;
        mwBotMessage.MessageType = MessageType.StartCharging;
        mwBotMessage.Status = MwBotStatus.ChargingCar;

        var currentlyCharging = await currentlyChargingRepository.GetByImmediateRequestId(mwBotMessage.ImmediateRequestId.Value);
        if (currentlyCharging == null)
        {
            _logger.LogWarning("No currently charging record found for MwBot {id}", mwBotMessage.Id);
            responseMessage.CurrentlyCharging = null;
        }
        else
        {
            responseMessage.CurrentlyCharging = currentlyCharging;
        }

        await PublishMessage(responseMessage);
    }
    
    private async Task HandleCompletedChargeMessageAsync(MqttClientMessage mwBotMessage, InterceptingPublishEventArgs arg, MwBotRepository mwBotRepository)
    {
        _logger.LogDebug("MqttBroker: MwBot {id} completed charging", mwBotMessage.Id);

        var scope = _serviceScopeFactory.CreateScope();
        var currentlyChargingRepository = scope.ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();
        var parkingSlotRepository = scope.ServiceProvider.GetRequiredService<ParkingSlotRepository>();

        mwBotMessage.CurrentlyCharging.EndChargingTime = DateTime.Now;
        mwBotMessage.CurrentlyCharging.ToPay = true;
        await currentlyChargingRepository.UpdateAsync(mwBotMessage.CurrentlyCharging);

        var parkingSlot = await parkingSlotRepository.GetByIdAsync(mwBotMessage.CurrentlyCharging.ParkingSlotId.Value);
        parkingSlot.Status = ParkingSlotStatus.Free;
        await parkingSlotRepository.UpdateAsync(parkingSlot);
        _logger.LogDebug("MqttBroker: Parking slot {id} is now free", parkingSlot.Id);
    }

    private async Task HandleChargeRequestMessageAsync(MqttClientMessage mwBotMessage, InterceptingPublishEventArgs arg, MwBotRepository mwBotRepository)
    {
        _logger.LogDebug("MqttBroker: MwBot {id} is charging car", mwBotMessage.Id);

        using var scope = _serviceScopeFactory.CreateScope();
        var parkingRepository = scope.ServiceProvider.GetRequiredService<ParkingRepository>();
        var currentlyChargingRepository = scope.ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();

        // TODO: car on parking slot
        var currentlyCharging = new CurrentlyCharging
        {
            TargetChargePercentage = mwBotMessage.TargetBatteryPercentage,
            MwBotId = mwBotMessage.Id,
            UserId = mwBotMessage.UserId,
            ParkingSlotId = mwBotMessage.ParkingSlotId,
            ImmediateRequestId = mwBotMessage.ImmediateRequestId.Value,
        };
        await currentlyChargingRepository.AddAsync(currentlyCharging);

        var parking = await parkingRepository.GetByIdAsync(mwBotMessage.ParkingId.Value);

        var confirmMessage = mwBotMessage;
        confirmMessage.MessageType = MessageType.StartCharging;
        confirmMessage.Status = MwBotStatus.ChargingCar;
        confirmMessage.CurrentlyCharging = currentlyCharging;
        confirmMessage.Parking = parking;

        await PublishMessage(confirmMessage);
        _logger.LogDebug("Created charging record + confirmation sent to MwBot {id}", mwBotMessage.Id);
    }

    public async Task PublishMessage(MqttClientMessage message)
    {
        var payload = JsonSerializer.Serialize(message);
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithPayload(payload)
            .WithTopic($"mwbot{message.Id}")
            .Build();

        await _mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(mqttMessage) { SenderClientId = "MqttServer" });
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