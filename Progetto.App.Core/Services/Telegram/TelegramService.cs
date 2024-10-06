using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Progetto.App.Core.Models.Users;
using Telegram.Bot.Polling;
using Telegram.Bot;
using Telegram.Bot.Types;

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
            await SendMessageAsync(chatId, "Welcome! Send me your verification code to link your account.");
        }
        // TODO: else if (messageText.StartsWith("/stopcharge"))
        // TODO: else if (messageText.StartsWith("/checkstatus"))
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
