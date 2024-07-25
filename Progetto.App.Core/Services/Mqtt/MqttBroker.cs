using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Server;
using Progetto.App.Core.Models;
using Progetto.App.Core.Models.Mqtt;
using Progetto.App.Core.Repositories;
using System.Text;
using System.Text.Json;
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
                    _logger.LogInformation("MqttBroker: MwBot {id} requested MessageType.RequestResumeCharging", mwBotMessage.Id);
                    await HandleResumeChargingRequestMessageAsync(mwBotMessage);
                    break;

                case MessageType.RequestCharge:
                    _logger.LogInformation("MqttBroker: MwBot {id} requested MessageType.RequestCharge", mwBotMessage.Id);
                    await HandleChargeRequestMessageAsync(mwBotMessage);
                    break;

                case MessageType.CompleteCharge:
                    _logger.LogInformation("MqttBroker: MwBot {id} requested MessageType.CompleteCharge", mwBotMessage.Id);
                    await HandleCompletedChargeMessageAsync(mwBotMessage, mwBotRepository);
                    break;

                case MessageType.RequestRecharge:
                    _logger.LogInformation("MqttBroker: MwBot {id} requested MessageType.RequestRecharge", mwBotMessage.Id);
                    await HandleRechargeRequestMessageAsync(mwBotMessage, mwBotRepository);
                    break;

                case MessageType.UpdateCharging:
                    _logger.LogInformation("MqttBroker: MwBot {id} requested MessageType.UpdateCharging", mwBotMessage.Id);
                    var currentlyChargingRepository = scope.ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();
                    await currentlyChargingRepository.UpdateAsync(mwBotMessage.CurrentlyCharging);
                    break;

                case MessageType.UpdateMwBot:
                    _logger.LogInformation("MqttBroker: MwBot {id} requested MessageType.UpdateMwBot", mwBotMessage.Id);
                    // Done by default
                    break;

                case MessageType.UpdateParkingSlot:
                    _logger.LogInformation("MqttBroker: MwBot {id} requested MessageType.UpdateParkingSlot", mwBotMessage.Id);
                    var parkingSlotRepository = scope.ServiceProvider.GetRequiredService<ParkingSlotRepository>();
                    var parkingSlot = await parkingSlotRepository.GetByIdAsync(mwBotMessage.ParkingSlotId.Value);
                    parkingSlot.Status = mwBotMessage.ParkingSlot.Status;
                    await parkingSlotRepository.UpdateAsync(parkingSlot);
                    break;

                case MessageType.DisconnectClient:
                    _logger.LogInformation("MqttBroker: MwBot {id} requested MessageType.DisconnectClient", mwBotMessage.Id);
                    await HandleDisconnectMessageAsync(mwBotMessage, mwBotRepository);
                    break;

                default:
                    _logger.LogDebug("MqttBroker: Invalid topic {topic}", arg.ApplicationMessage.Topic);
                    break;

            }

            _logger.LogDebug("MqttBroker: Updating MwBot {id} with params: {battery} and {status}", mwBot.Id, mwBotMessage.BatteryPercentage, mwBot.Status);
            mwBot.Status = mwBotMessage.Status;
            mwBot.BatteryPercentage = mwBotMessage.BatteryPercentage;
            await mwBotRepository.UpdateAsync(mwBot);
        }

        await Task.CompletedTask;
    }

    private async Task HandleDisconnectMessageAsync(MqttClientMessage mwBotMessage, MwBotRepository mwBotRepository)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var parkingSlotRepository = scope.ServiceProvider.GetRequiredService<ParkingSlotRepository>();
        var currentlyChargingRepository = scope.ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();

        var mwBot = await mwBotRepository.GetByIdAsync(mwBotMessage.Id);
        if (mwBot is null)
        {
            _logger.LogWarning("MwBot not found");
            return;
        }
        mwBot.Status = MwBotStatus.Offline;
        await mwBotRepository.UpdateAsync(mwBot);

        var currentlyChargingList = await currentlyChargingRepository.GetAllActiveByMwBot(mwBotMessage.Id);
        foreach (var currentlyCharging in currentlyChargingList)
        {
            var parkingSlot = await parkingSlotRepository.GetByIdAsync(currentlyCharging.ParkingSlotId.Value);
            parkingSlot.Status = ParkingSlotStatus.Free;
            await parkingSlotRepository.UpdateAsync(parkingSlot);
        }

        _logger.LogDebug("MqttBroker: MwBot {id} disconnected", mwBotMessage.Id);
    }

    private async Task HandleRechargeRequestMessageAsync(MqttClientMessage mwBotMessage, MwBotRepository mwBotRepository)
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
        _logger.LogDebug("MqttBroker: Confirmation sent to MwBot {id} to start recharging", mwBotMessage.Id);
    }

    private async Task HandleResumeChargingRequestMessageAsync(MqttClientMessage mwBotMessage)
    {
        _logger.LogDebug("MqttBroker: MwBot {id} requested to resume charging", mwBotMessage.Id);

        using var scope = _serviceScopeFactory.CreateScope();
        var currentlyChargingRepository = scope.ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();
        var parkingRepository = scope.ServiceProvider.GetRequiredService<ParkingRepository>();
        var immediateRequestRepository = scope.ServiceProvider.GetRequiredService<ImmediateRequestRepository>();

        var parking = await parkingRepository.GetByIdAsync(mwBotMessage.ParkingId.Value);
        if (parking is null)
        {
            _logger.LogWarning("MqttBroker: Parking not found");
            return;
        }

        ImmediateRequest? immediateRequest = null;
        var currentlyCharging = await currentlyChargingRepository.GetActiveByMwBot(mwBotMessage.Id);
        if (currentlyCharging is not null)
        {
            immediateRequest = await immediateRequestRepository.GetByIdAsync(currentlyCharging.ImmediateRequestId.Value);
            if (immediateRequest is null)
            {
                _logger.LogWarning("MqttBroker: Immediate request not found");
            }
        }

        mwBotMessage.MessageType = MessageType.ResumeCharging;
        mwBotMessage.Status = MwBotStatus.ChargingCar;
        mwBotMessage.Parking = parking;
        mwBotMessage.ImmediateRequest = immediateRequest;
        mwBotMessage.CurrentlyCharging = currentlyCharging;
        mwBotMessage.CarPlate = currentlyCharging?.CarPlate;
        mwBotMessage.CurrentCarCharge = currentlyCharging?.CurrentChargePercentage;
        mwBotMessage.ParkingSlotId = currentlyCharging?.ParkingSlotId;
        mwBotMessage.TargetBatteryPercentage = currentlyCharging?.TargetChargePercentage;
        mwBotMessage.UserId = currentlyCharging?.UserId;

        await PublishMessage(mwBotMessage);
    }

    private async Task HandleCompletedChargeMessageAsync(MqttClientMessage mwBotMessage, MwBotRepository mwBotRepository)
    {
        _logger.LogDebug("MqttBroker: MwBot {id} completed charging", mwBotMessage.Id);

        using var scope = _serviceScopeFactory.CreateScope();
        var currentlyChargingRepository = scope.ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();
        var immediateRequestRepository = scope.ServiceProvider.GetRequiredService<ImmediateRequestRepository>();
        var parkingSlotRepository = scope.ServiceProvider.GetRequiredService<ParkingSlotRepository>();
        var parkingRepository = scope.ServiceProvider.GetRequiredService<ParkingRepository>();
        var carRepository = scope.ServiceProvider.GetRequiredService<CarRepository>();

        await immediateRequestRepository.DeleteAsync(ir => ir.Id == mwBotMessage.ImmediateRequestId);

        var parking = await parkingRepository.GetByIdAsync(mwBotMessage.ParkingId.Value);
        if (parking is null)
        {
            _logger.LogWarning("MqttBroker: Parking not found");
            return;
        }

        mwBotMessage.CurrentlyCharging.ImmediateRequestId = null;
        mwBotMessage.CurrentlyCharging.EndChargingTime = DateTime.Now;
        mwBotMessage.CurrentlyCharging.ToPay = true;
        mwBotMessage.Parking = parking;
        await currentlyChargingRepository.UpdateAsync(mwBotMessage.CurrentlyCharging);
        await carRepository.UpdateCarStatus(mwBotMessage.CarPlate, CarStatus.Charged);

        var parkingSlot = await parkingSlotRepository.GetByIdAsync(mwBotMessage.CurrentlyCharging.ParkingSlotId.Value);
        parkingSlot.Status = ParkingSlotStatus.Free;
        await parkingSlotRepository.UpdateAsync(parkingSlot);

        _logger.LogDebug("MqttBroker: Parking slot {id} is now free", parkingSlot.Id);
    }

    private async Task HandleChargeRequestMessageAsync(MqttClientMessage mwBotMessage)
    {
        _logger.LogDebug("MqttBroker: MwBot {id} is charging car", mwBotMessage.Id);

        using var scope = _serviceScopeFactory.CreateScope();
        var carRepository = scope.ServiceProvider.GetRequiredService<CarRepository>();
        var parkingRepository = scope.ServiceProvider.GetRequiredService<ParkingRepository>();
        var currentlyChargingRepository = scope.ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();

        var currentlyCharging = await currentlyChargingRepository.GetActiveByImmediateRequest(mwBotMessage.ImmediateRequestId.Value);
        if (currentlyCharging == null) // record doesn't exist, create new
        {
            currentlyCharging = new CurrentlyCharging
            {
                TargetChargePercentage = mwBotMessage.TargetBatteryPercentage,
                MwBotId = mwBotMessage.Id,
                UserId = mwBotMessage.UserId,
                ParkingSlotId = mwBotMessage.ParkingSlotId,
                ImmediateRequestId = mwBotMessage.ImmediateRequestId.Value,
                CarPlate = mwBotMessage.CarPlate,
                CurrentChargePercentage = 0,
                StartChargePercentage = 0,
            };
            await currentlyChargingRepository.AddAsync(currentlyCharging);
        }
        else // record exists, update it
        {
            currentlyCharging.CurrentChargePercentage = mwBotMessage.BatteryPercentage;
            currentlyCharging.TargetChargePercentage = mwBotMessage.TargetBatteryPercentage;
            await currentlyChargingRepository.UpdateAsync(currentlyCharging);
        }

        await carRepository.UpdateCarStatus(mwBotMessage.CarPlate, CarStatus.InCharge);

        var confirmMessage = mwBotMessage;
        confirmMessage.CurrentlyCharging = currentlyCharging;

        var parking = await parkingRepository.GetByIdAsync(mwBotMessage.ParkingId.Value);
        confirmMessage.MessageType = MessageType.StartCharging;
        confirmMessage.Status = MwBotStatus.ChargingCar;
        confirmMessage.Parking = parking;

        await PublishMessage(confirmMessage);
        _logger.LogDebug("MqttBroker: Created charging record + confirmation sent to MwBot {id}", mwBotMessage.Id);
    }

    public async Task PublishMessage(MqttClientMessage message)
    {
        try
        {
            var payload = JsonSerializer.Serialize(message);
            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithPayload(payload)
                .WithTopic($"mwbot{message.Id}")
                .Build();

            await _mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(mqttMessage) { SenderClientId = "MqttServer" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MqttBroker: Error publishing message");
        }
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