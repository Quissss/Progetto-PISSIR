using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Server;
using Progetto.App.Core.Models;
using Progetto.App.Core.Models.Mqtt;
using Progetto.App.Core.Models.Users;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Services.Telegram;
using System.Text;
using System.Text.Json;
using static System.Formats.Asn1.AsnWriter;

namespace Progetto.App.Core.Services.Mqtt;

/// <summary>
/// MwBot's MQTT broker (server)
/// </summary>
public class MqttBroker : IHostedService, IDisposable
{
    private readonly TelegramService _telegramService;
    private readonly ChargeManager _chargeManager;
    private readonly MqttServer _mqttServer;
    private readonly MqttServerOptions _options;
    private readonly ILogger<MqttBroker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MqttBroker(TelegramService telegramService, ChargeManager chargeManager, ILogger<MqttBroker> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        _options = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(configuration.GetValue<int>("MqttPort"))
            .WithKeepAlive()
            .Build();

        _telegramService = telegramService;
        _chargeManager = chargeManager;
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
                case MessageType.RequestCharge:
                    _logger.LogDebug("MqttBroker: MwBot {id} requested MessageType.RequestCharge", mwBotMessage.Id);
                    await HandleChargeRequestMessageAsync(mwBotMessage);
                    break;

                case MessageType.CompleteCharge:
                    _logger.LogDebug("MqttBroker: MwBot {id} requested MessageType.CompleteCharge", mwBotMessage.Id);
                    await HandleCompletedChargeMessageAsync(mwBotMessage);
                    break;

                case MessageType.RequestRecharge:
                    _logger.LogDebug("MqttBroker: MwBot {id} requested MessageType.RequestRecharge", mwBotMessage.Id);
                    await HandleRechargeRequestMessageAsync(mwBotMessage, mwBotRepository);
                    break;

                case MessageType.UpdateCharging:
                    _logger.LogDebug("MqttBroker: MwBot {id} requested MessageType.UpdateCharging", mwBotMessage.Id);
                    var currentlyChargingRepository = scope.ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();
                    await currentlyChargingRepository.UpdateAsync(mwBotMessage.CurrentlyCharging);
                    break;

                case MessageType.RequestMwBot:
                    mwBotMessage.BatteryPercentage = mwBot.BatteryPercentage;
                    mwBotMessage.ParkingId = mwBot.ParkingId;

                    // BADFIX desync with database for some reason
                    mwBotMessage.Status = mwBot.Status = MwBotStatus.StandBy;
                    await HandleRequestMwBotMessageAsync(mwBot);
                    break;

                case MessageType.UpdateMwBot:
                    _logger.LogDebug("MqttBroker: MwBot {id} requested MessageType.UpdateMwBot", mwBotMessage.Id);
                    // Done by default
                    break;

                case MessageType.UpdateParkingSlot:
                    _logger.LogDebug("MqttBroker: MwBot {id} requested MessageType.UpdateParkingSlot", mwBotMessage.Id);
                    var parkingSlotRepository = scope.ServiceProvider.GetRequiredService<ParkingSlotRepository>();
                    var parkingSlot = await parkingSlotRepository.GetByIdAsync(mwBotMessage.ParkingSlotId.Value);
                    parkingSlot.Status = mwBotMessage.ParkingSlot.Status;
                    await parkingSlotRepository.UpdateAsync(parkingSlot);
                    break;

                case MessageType.DisconnectClient:
                    _logger.LogDebug("MqttBroker: MwBot {id} requested MessageType.DisconnectClient", mwBotMessage.Id);
                    await HandleDisconnectMessageAsync(mwBotMessage, mwBotRepository);
                    break;

                default:
                    _logger.LogDebug("MqttBroker: Invalid topic {topic}", arg.ApplicationMessage.Topic);
                    break;

            }

            mwBot.Status = mwBotMessage.Status;
            mwBot.BatteryPercentage = mwBotMessage.BatteryPercentage;
            mwBot.LatestLocation = mwBotMessage.LatestLocation;

            _logger.LogDebug("MqttBroker: Updating MwBot {id} with battery: {battery}% and status: {status} and location: {location}", mwBot.Id, mwBotMessage.BatteryPercentage, mwBot.Status, mwBot.LatestLocation);
            await mwBotRepository.UpdateAsync(mwBot);
        }

        await Task.CompletedTask;
    }

    private async Task HandleRequestMwBotMessageAsync(MwBot mwBot)
    {
        var responseMessage = new MqttClientMessage
        {
            MessageType = MessageType.ReturnMwBot,
            Id = mwBot.Id,
            BatteryPercentage = mwBot.BatteryPercentage,
            Status = mwBot.Status,
            ParkingId = mwBot.ParkingId,
            Parking = mwBot.Parking
        };

        await PublishMessage(responseMessage);
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

    private async Task HandleChargeRequestMessageAsync(MqttClientMessage mwBotMessage)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var carRepository = scope.ServiceProvider.GetRequiredService<CarRepository>();
        var mwBotRepository = scope.ServiceProvider.GetRequiredService<MwBotRepository>();
        var parkingRepository = scope.ServiceProvider.GetRequiredService<ParkingRepository>();
        var immediateRequestRepository = scope.ServiceProvider.GetRequiredService<ImmediateRequestRepository>();
        var currentlyChargingRepository = scope.ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();

        var mwBot = await mwBotRepository.GetByIdAsync(mwBotMessage.Id);
        if (mwBot == null)
        {
            _logger.LogWarning("MqttBroker: MwBot not found");
            return;
        }

        CurrentlyCharging? currentlyCharging = null;
        ImmediateRequest? immediateRequest = null;

        // Check if the bot has a currently charging session
        currentlyCharging = await currentlyChargingRepository.GetActiveByMwBot(mwBot.Id);
        if (currentlyCharging != null)
        {
            // If there's an active charging session, get the associated immediate request
            if (currentlyCharging.ImmediateRequestId.HasValue)
            {
                immediateRequest = await immediateRequestRepository.GetByIdAsync(currentlyCharging.ImmediateRequestId.Value);
                //await SendTelegramMessage($"Resuming charge for car {immediateRequest.CarPlate}", scope, immediateRequest.UserId);
            }

            if (immediateRequest == null)
            {
                _logger.LogWarning("MqttBroker: Immediate request not found for currently charging record");
            }
        }

        // If there's no active charging session, or we couldn't find an associated immediate request
        if (currentlyCharging == null || immediateRequest == null)
        {
            // Attempt to get a new immediate request from the charge manager
            immediateRequest = await _chargeManager.ServeNext(mwBot);
            if (immediateRequest != null)
            {
                // Create a new currently charging record if a new request is assigned
                currentlyCharging = new CurrentlyCharging
                {
                    TargetChargePercentage = immediateRequest.RequestedChargeLevel,
                    MwBotId = mwBot.Id,
                    UserId = immediateRequest.UserId,
                    ParkingSlotId = immediateRequest.ParkingSlotId,
                    ImmediateRequestId = immediateRequest.Id,
                    CarPlate = immediateRequest.CarPlate,
                    StartChargingTime = DateTime.Now,
                    CurrentChargePercentage = 0,
                    StartChargePercentage = 0,
                };
                await currentlyChargingRepository.AddAsync(currentlyCharging);
                await SendTelegramMessage($"Starting charge for car {currentlyCharging.CarPlate}", scope, currentlyCharging.UserId);
            }
            else
            {
                _logger.LogDebug("MqttBroker: No immediate request available for MwBot {id}", mwBot.Id);
                return;
            }
        }

        // Update car status
        await carRepository.UpdateCarStatus(currentlyCharging.CarPlate, CarStatus.InCharge, currentlyCharging.ParkingSlotId);

        // Update MwBot status
        mwBot.Status = MwBotStatus.ChargingCar;
        mwBotMessage.LatestLocation = mwBot.LatestLocation = MwBotLocations.InSlot;
        await mwBotRepository.UpdateAsync(mwBot);

        // Prepare and publish the response message
        mwBotMessage.MessageType = MessageType.StartCharging;
        mwBotMessage.Status = MwBotStatus.ChargingCar;
        mwBotMessage.ImmediateRequest = immediateRequest;
        mwBotMessage.CurrentlyCharging = currentlyCharging;
        mwBotMessage.CarPlate = currentlyCharging.CarPlate;
        mwBotMessage.BatteryPercentage = mwBot.BatteryPercentage;
        mwBotMessage.TargetBatteryPercentage = currentlyCharging.TargetChargePercentage;
        mwBotMessage.ParkingSlotId = currentlyCharging.ParkingSlotId;
        mwBotMessage.Parking = await parkingRepository.GetByIdAsync(mwBot.ParkingId.Value);

        await PublishMessage(mwBotMessage);
    }

    private async Task HandleCompletedChargeMessageAsync(MqttClientMessage mwBotMessage)
    {

        using var scope = _serviceScopeFactory.CreateScope();
        var currentlyChargingRepository = scope.ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();
        var immediateRequestRepository = scope.ServiceProvider.GetRequiredService<ImmediateRequestRepository>();
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
        mwBotMessage.CurrentlyCharging.TotalCost = Math.Round((decimal)mwBotMessage.CurrentlyCharging.TotalCost, 2);
        mwBotMessage.Parking = parking;
        await currentlyChargingRepository.UpdateAsync(mwBotMessage.CurrentlyCharging);
        await carRepository.UpdateCarStatus(mwBotMessage.CarPlate, CarStatus.Charged);

        await SendTelegramMessage($"Your car with plate {mwBotMessage.CurrentlyCharging.CarPlate} has been charged at {parking.Name}.\n" +
                          $"Total cost: {mwBotMessage.CurrentlyCharging.TotalCost}€\n" +
                          $"[Pay now](https://localhost:7237/payments)", scope, mwBotMessage.CurrentlyCharging.UserId);

        mwBotMessage.MessageType = MessageType.ChargeCompleted;
        await PublishMessage(mwBotMessage);
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
    /// Sends a Telegram message to the user
    /// </summary>
    /// <param name="message"></param>
    /// <param name="scope"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    private async Task SendTelegramMessage(string message, IServiceScope scope, string userId)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId);
        try
        {
            if (user.IsTelegramNotificationEnabled && user.TelegramChatId.HasValue)
            {
                await _telegramService.SendMessageAsync(user.TelegramChatId.Value, message);
                _logger.LogInformation("MqttBroker: Telegram message sent to chat ID {chatId}", user.TelegramChatId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MqttBroker: Error sending Telegram message");
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
        GC.SuppressFinalize(this);
    }
}