using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Progetto.App.Core.Models.Users;
using Telegram.Bot.Polling;
using Telegram.Bot;
using Telegram.Bot.Types;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Core.Services.Telegram;

public class TelegramService
{
    private readonly string _botToken;
    private readonly TelegramBotClient _botClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TelegramService(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
    {
        _botToken = configuration["Telegram:BotToken"];
        _botClient = new TelegramBotClient(_botToken);
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task SendMessageAsync(long chatId, string message)
    {
        if (chatId == 0 || string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Chat ID e messaggio sono obbligatori.");
        }

        await _botClient.SendTextMessageAsync(chatId, message);
    }

    public void StartReceivingUpdates()
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { },
        };

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandleErrorAsync,
            receiverOptions: receiverOptions
        );
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update == null || update.Message == null)
        {
            return;
        }

        var chatId = update.Message.Chat.Id;
        var messageText = update.Message.Text;

        if (messageText.StartsWith("/start"))
        {
            await SendMessageAsync(chatId, "Welcome! If you haven't linked your account, send me your verification code.\n\n" +
                "Our commands:\n" +
                "/checkstatus [carplate] - retrieves charge status\n" +
                "/chargestatus [carplate] - retrieves charge status");
        }
        // TODO: else if (messageText.StartsWith("/stopcharge"))
        else if (messageText.StartsWith("/checkstatus") || messageText.StartsWith("/chargestatus"))
        {
            await SendMessageAsync(chatId, "Checking status...");
            var carPlate = messageText.Split(" ").Skip(1).FirstOrDefault();

            if (string.IsNullOrEmpty(carPlate))
            {
                await SendMessageAsync(chatId, "Please, provide a car plate.");
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var currentlyChargingRepository = scope.ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();

            var currentlyCharging = await currentlyChargingRepository.GetChargingByCarPlate(carPlate);

            if (currentlyCharging == null)
            {
                await SendMessageAsync(chatId, $"No active charge found for car plate {carPlate}.");
                return;
            }

            var responseMessage = $"Active charge for car {carPlate}:\n" +
                                    $"- Started at: {currentlyCharging.StartChargingTime}\n" +
                                    $"- Current charge: {currentlyCharging.CurrentChargePercentage}% / {currentlyCharging.TargetChargePercentage}%\n" +
                                    $"- Energy consumed: {currentlyCharging.EnergyConsumed} kWh\n" +
                                    $"- Total cost: {currentlyCharging.TotalCost} EUR";

            await SendMessageAsync(chatId, responseMessage);
        }
        else
        {
            await HandleVerificationCodeAsync(chatId, messageText);
        }
    }

    private async Task HandleVerificationCodeAsync(long chatId, string verificationCode)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.TelegramVerificationCode == verificationCode);

        if (user != null)
        {
            // Set chat ID and clear verification code
            user.TelegramChatId = chatId;
            user.TelegramVerificationCode = null;
            await userManager.UpdateAsync(user);

            await SendMessageAsync(chatId, "Your account has been successfully linked to the bot!");
        }
        else
        {
            await SendMessageAsync(chatId, "Verification code invalid. Please, check it and try again.");
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Bot error: {exception.Message}");
        return Task.CompletedTask;
    }
}
