﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Services.SignalR.Hubs;

namespace Progetto.App.Core.Services.Mqtt;


public class ConnectedClientsService
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IHubContext<MwBotHub> _mwBotHubContext;
    private readonly ILogger<ConnectedClientsService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly List<MqttMwBotClient> _connectedClients;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectedClientsService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceScopeFactory">The service scope factory.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="mwBotHubContext">The mw bot hub context.</param>
    public ConnectedClientsService(ILogger<ConnectedClientsService> logger, IServiceScopeFactory serviceScopeFactory, ILoggerFactory loggerFactory, IHubContext<MwBotHub> mwBotHubContext)
    {
        _logger = logger;
        _mwBotHubContext = mwBotHubContext;
        _serviceScopeFactory = serviceScopeFactory;
        _loggerFactory = loggerFactory;
        _connectedClients = new List<MqttMwBotClient>();
    }

    /// <summary>
    /// Initializes the connected clients.
    /// </summary>
    public async Task InitializeConnectedClients()
    {
        _logger.BeginScope("Retrieving connected clients");
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mwBotRepository = scope.ServiceProvider.GetRequiredService<MwBotRepository>();
            var mwBotList = await mwBotRepository.GetOnlineMwBots();

            foreach (var singleMwBot in mwBotList)
            {
                if (_connectedClients.Any(c => c.MwBot?.Id == singleMwBot.Id))
                {
                    _logger.LogWarning("Client for MwBot {id} already exists. Skipping initialization.", singleMwBot.Id);
                    continue;
                }

                _logger.LogDebug("Creating MqttMwBotClient for MwBot with id {id}", singleMwBot.Id);
                var client = new MqttMwBotClient(
                    _loggerFactory.CreateLogger<MqttMwBotClient>(),
                    _serviceScopeFactory);

                var connectResult = await client.InitializeAsync(singleMwBot.Id);
                if (!connectResult) // If connection fails, set MwBot status to offline
                {
                    _logger.LogError("Failed to connect MwBot with id {id} to MQTT server while getting connected clients", singleMwBot.Id);
                    var mwBot = await mwBotRepository.UpdateMwBotStatus(singleMwBot.Id, MwBotStatus.Offline);
                    await _mwBotHubContext.Clients.All.SendAsync("MwBotUpdated", mwBot);
                }
                else
                {
                    _connectedClients.Add(client);
                }
            }
        }
        catch
        {
            _logger.LogError("Error while retrieving online MwBots");
        }

        _logger.LogDebug("Connected clients retrieved");
    }

    /// <summary>
    /// Gets the connected clients.
    /// </summary>
    /// <returns></returns>
    public List<MqttMwBotClient> GetConnectedClients()
    {
        return _connectedClients;
    }

    /// <summary>
    /// Adds the client.
    /// </summary>
    /// <param name="client">The client.</param>
    public void AddClient(MqttMwBotClient client)
    {
        _connectedClients.Add(client);
    }

    /// <summary>
    /// Removes the client.
    /// </summary>
    /// <param name="client">The client.</param>
    public void RemoveClient(MqttMwBotClient client)
    {
        if (_connectedClients.Remove(client))
        {
            client.Dispose();
            _logger.LogInformation("Client for MwBot {id} removed and disposed.", client.MwBot?.Id);
        }
    }
}
