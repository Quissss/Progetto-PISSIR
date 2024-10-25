using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
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
using Progetto.App.Core.Services.SignalR.Hubs;
using Progetto.App.Core.Services.Telegram;
using System.Configuration;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

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
    private readonly IHubContext<CarHub> _carHub;
    private readonly IHubContext<MwBotHub> _botHub;
    private readonly IHubContext<RechargeHub> _rechargeHub;
    private readonly IHubContext<ParkingSlotHub> _parkingSlotHub;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient = new();

    public MqttBroker(TelegramService telegramService, ChargeManager chargeManager, ILogger<MqttBroker> logger, IHubContext<CarHub> carHub, IHubContext<MwBotHub> botHubContext, IHubContext<RechargeHub> rechargeHubContext, IHubContext<ParkingSlotHub> parkingSlotHubContext, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        _options = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(configuration.GetValue<int>("Mqtt:Port"))
            .WithKeepAlive()
            .Build();

        _logger = logger;
        _carHub = carHub;
        _botHub = botHubContext;
        _configuration = configuration;
        _rechargeHub = rechargeHubContext;
        _parkingSlotHub = parkingSlotHubContext;
        _serviceScopeFactory = serviceScopeFactory;
        _telegramService = telegramService;
        _chargeManager = chargeManager;

        _mqttServer = new MqttFactory().CreateMqttServer(_options);

        _mqttServer.ClientConnectedAsync += MqttServer_ClientConnectedAsync;
        _mqttServer.ClientDisconnectedAsync += MqttServer_ClientDisconnectedAsync;
        _mqttServer.InterceptingPublishAsync += MqttServer_InterceptingPublishAsync;
    }

    /// <summary>
    /// Changes hue light state
    /// </summary>
    /// <param name="lightId"></param>
    /// <param name="requestBody"></param>
    /// <returns></returns>
    private async Task ChangeLightState(string lightId, string requestBody)
    {
        string? baseUrl = _configuration.GetValue<string>("BaseUrl") + ":" + _configuration.GetValue<int>("Hue:Port") 
                            ?? throw new ConfigurationErrorsException("Hue base URL not found");
        string? username = _configuration.GetValue<string>("Hue:Username") 
                            ?? throw new ConfigurationErrorsException("Hue username not found");

        string lightsUrl = $"{baseUrl}/api/{username}/lights/";

        var url = $"{lightsUrl}{lightId}/state";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        await _httpClient.PutAsync(url, content);
    }

    private async Task HandleHueLights(MqttClientMessage mwBotMessage)
    {
        switch (mwBotMessage.Status)
        {
            case MwBotStatus.Offline:
                await ChangeLightState(mwBotMessage.Id.ToString(), "{ \"on\": false }");
                break;
            case MwBotStatus.StandBy:
                await ChangeLightState(mwBotMessage.Id.ToString(), "{\"on\":true,\"sat\":0,\"bri\":254,\"hue\":0}");
                break;
            case MwBotStatus.ChargingCar:
                await ChangeLightState(mwBotMessage.Id.ToString(), "{\"on\":true,\"sat\":254,\"bri\":254,\"hue\":25500}");
                break;
            case MwBotStatus.Recharging:
                await ChangeLightState(mwBotMessage.Id.ToString(), "{\"on\":true,\"sat\":254,\"bri\":254,\"hue\":46920}");
                break;
            case MwBotStatus.MovingToSlot:
                await ChangeLightState(mwBotMessage.Id.ToString(), "{\"on\":true,\"sat\":0,\"bri\":154,\"hue\":25500}");
                break;
            case MwBotStatus.MovingToDock:
                await ChangeLightState(mwBotMessage.Id.ToString(), "{\"on\":true,\"sat\":0,\"bri\":154,\"hue\":46920}");
                break;
            default:
                await ChangeLightState(mwBotMessage.Id.ToString(), "{\"on\":true,\"sat\":254,\"bri\":254,\"hue\":0}");
                break;
        }
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
        //_logger.LogDebug("MqttBroker: MwBot message: {mwBotMessage}", mwBotMessage);

        if (mwBotMessage != null)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mwBotRepository = scope.ServiceProvider.GetRequiredService<MwBotRepository>();
            _logger.BeginScope("MqttBroker: Handling message by {id}", mwBotMessage.Id);

            var mwBot = await mwBotRepository.GetByIdAsync(mwBotMessage.Id);
            if (mwBot is null)
            {
                _logger.LogWarning("MqttBroker: MwBot doesn't exist");
                return;
            }

            await HandleHueLights(mwBotMessage);
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
                    await _rechargeHub.Clients.All.SendAsync("RechargeUpdated", mwBotMessage.CurrentlyCharging);
                    break;

                case MessageType.RequestMwBot:
                    mwBotMessage.BatteryPercentage = mwBot.BatteryPercentage;
                    mwBotMessage.ParkingId = mwBot.ParkingId;
                    mwBotMessage.LatestLocation = mwBot.LatestLocation;

                    // BADFIX desync with database for some reason
                    mwBotMessage.Status = mwBot.Status = MwBotStatus.StandBy;
                    await HandleRequestMwBotMessageAsync(mwBot);
                    break;

                case MessageType.UpdateMwBot:
                    _logger.LogDebug("MqttBroker: MwBot {id} requested MessageType.UpdateMwBot", mwBotMessage.Id);
                    // Done by default
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

            _logger.LogDebug("MqttBroker: Updating MwBot {id} with battery: {battery}% | status: {status} | location: {location}", mwBot.Id, mwBotMessage.BatteryPercentage, mwBot.Status, mwBot.LatestLocation);
            await mwBotRepository.UpdateAsync(mwBot);
            await _botHub.Clients.All.SendAsync("MwBotUpdated", mwBot);
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
            Parking = mwBot.Parking,
            LatestLocation = mwBot.LatestLocation
        };

        await PublishMessage(responseMessage);
    }

    private async Task HandleDisconnectMessageAsync(MqttClientMessage mwBotMessage, MwBotRepository mwBotRepository)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var parkingSlotRepository = scope.ServiceProvider.GetRequiredService<ParkingSlotRepository>();
        var currentlyChargingRepository = scope.ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();

        var mwBot = await mwBotRepository.UpdateMwBotStatus(mwBotMessage.Id, MwBotStatus.Offline);
        await _botHub.Clients.All.SendAsync("MwBotUpdated", mwBot);

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

        if (mwBot.LatestLocation != MwBotLocations.InDock)
        {
            mwBot = await mwBotRepository.UpdateMwBotStatus(mwBot.Id, MwBotStatus.MovingToDock);
            await _botHub.Clients.All.SendAsync("MwBotUpdated", mwBot);
        }

        mwBotMessage.MessageType = MessageType.StartRecharge;
        mwBotMessage.Id = mwBot.Id;
        mwBotMessage.Status = mwBot.Status;
        mwBotMessage.BatteryPercentage = mwBot.BatteryPercentage;
        mwBotMessage.LatestLocation = mwBot.LatestLocation;

        await PublishMessage(mwBotMessage);
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

        // Check if the bot has a currently charging session or there's a free charging session to progress
        currentlyCharging = await currentlyChargingRepository.GetActiveByMwBot(mwBot.Id);
        currentlyCharging ??= await currentlyChargingRepository.GetActiveWithNoBotByParking(mwBot.ParkingId.Value);
        if (currentlyCharging != null)
        {
            // If the charging session is not assigned to the bot, assign it
            if (currentlyCharging.MwBotId is null)
            {
                currentlyCharging.MwBotId = mwBot.Id;
                await currentlyChargingRepository.UpdateAsync(currentlyCharging);
            }

            // If there's an active charging session, get the associated immediate request
            if (currentlyCharging.ImmediateRequestId.HasValue)
            {
                immediateRequest = await immediateRequestRepository.GetByIdAsync(currentlyCharging.ImmediateRequestId.Value);
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
                _logger.LogDebug("MqttBroker: Immediate request found for MwBot {id}", mwBot.Id);

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
                await _rechargeHub.Clients.All.SendAsync("RechargeAdded", currentlyCharging);

                immediateRequest.IsBeingHandled = true;
                await immediateRequestRepository.UpdateAsync(immediateRequest);

                await SendTelegramMessage($"Starting charge for car {currentlyCharging.CarPlate}", scope, currentlyCharging.UserId);
            }
            else
            {
                _logger.LogDebug("MqttBroker: No immediate request available for MwBot {id}", mwBot.Id);
                return;
            }
        }

        // Update car status
        var car = await carRepository.UpdateCarStatus(currentlyCharging.CarPlate, CarStatus.InCharge, currentlyCharging.ParkingSlotId);
        await _carHub.Clients.All.SendAsync("CarUpdated", car);

        // Update MwBot status
        mwBot = await mwBotRepository.UpdateMwBotStatus(mwBot.Id, MwBotStatus.ChargingCar);
        await _botHub.Clients.All.SendAsync("MwBotUpdated", mwBot);

        // Prepare and publish the response message
        mwBotMessage.MessageType = MessageType.StartCharging;
        mwBotMessage.Status = MwBotStatus.ChargingCar;
        mwBotMessage.ImmediateRequest = immediateRequest;
        mwBotMessage.UserId = immediateRequest.UserId;
        mwBotMessage.CurrentlyCharging = currentlyCharging;
        mwBotMessage.CarPlate = currentlyCharging.CarPlate;
        mwBotMessage.BatteryPercentage = mwBot.BatteryPercentage;
        mwBotMessage.TargetBatteryPercentage = currentlyCharging.TargetChargePercentage;
        mwBotMessage.ParkingSlotId = currentlyCharging.ParkingSlotId;
        mwBotMessage.Parking = await parkingRepository.GetByIdAsync(mwBot.ParkingId.Value);
        mwBotMessage.LatestLocation = mwBot.LatestLocation;

        await PublishMessage(mwBotMessage);
    }

    private async Task HandleCompletedChargeMessageAsync(MqttClientMessage mwBotMessage)
    {

        using var scope = _serviceScopeFactory.CreateScope();
        var currentlyChargingRepository = scope.ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();
        var immediateRequestRepository = scope.ServiceProvider.GetRequiredService<ImmediateRequestRepository>();
        var parkingRepository = scope.ServiceProvider.GetRequiredService<ParkingRepository>();
        var carRepository = scope.ServiceProvider.GetRequiredService<CarRepository>();

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
        mwBotMessage.LatestLocation = MwBotLocations.InSlot;
        await currentlyChargingRepository.UpdateAsync(mwBotMessage.CurrentlyCharging);
        await _rechargeHub.Clients.All.SendAsync("RechargeUpdated", mwBotMessage.CurrentlyCharging);

        var car = await carRepository.UpdateCarStatus(mwBotMessage.CarPlate, CarStatus.Charged);
        await _carHub.Clients.All.SendAsync("CarUpdated", car);

        await immediateRequestRepository.DeleteAsync(ir => ir.Id == mwBotMessage.ImmediateRequestId);

        await SendTelegramMessage($"Your car with plate {mwBotMessage.CurrentlyCharging.CarPlate} has been charged at {parking.Name}.\n" +
                          $"Total cost: {mwBotMessage.CurrentlyCharging.TotalCost}€\n" +
                          $"[Pay now](https://localhost:7237/payments)\n\n" +
                          $"** link not clickable because it's localhost **", scope, mwBotMessage.CurrentlyCharging.UserId);

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

    public async Task SendStopChargeToBot(CurrentlyCharging currentCharge)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var mwBotRepository = scope.ServiceProvider.GetRequiredService<MwBotRepository>();
        var mwBot = await mwBotRepository.GetByIdAsync(currentCharge.MwBotId.Value);

        var mwBotMessage = new MqttClientMessage
        {
            MessageType = MessageType.StopCharging,
            Id = currentCharge.MwBotId.Value,
            Status = MwBotStatus.StandBy,
            CurrentlyCharging = currentCharge,
            CarPlate = currentCharge.CarPlate,
            ParkingSlotId = currentCharge.ParkingSlotId,
            UserId = currentCharge.UserId,
            BatteryPercentage = mwBot.BatteryPercentage,
            TargetBatteryPercentage = currentCharge.TargetChargePercentage,
            ParkingId = mwBot.ParkingId,
            Parking = mwBot.Parking,
            LatestLocation = mwBot.LatestLocation
        };

        await PublishMessage(mwBotMessage);
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
            if (_telegramService != null && user != null && user.IsTelegramNotificationEnabled && user.TelegramChatId.HasValue)
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