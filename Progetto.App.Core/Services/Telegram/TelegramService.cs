using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Progetto.App.Core.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Services.Telegram;

public class TelegramService
{
    private readonly string _botToken;
    private readonly HttpClient _httpClient;

    public TelegramService(IConfiguration configuration)
    {
        _botToken = configuration["Telegram:BotToken"];
        _httpClient = new HttpClient();
    }

    public async Task SendMessageAsync(string chatId, string message)
    {
        if (string.IsNullOrWhiteSpace(chatId) || string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Chat ID and message are required.");
        }

        var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("chat_id", chatId),
            new KeyValuePair<string, string>("text", message)
        });
        await _httpClient.PostAsync(url, content);
    }
}
