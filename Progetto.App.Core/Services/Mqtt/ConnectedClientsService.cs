using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Services.MQTT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Services.Mqtt;

public class ConnectedClientsService
{
    private readonly ILogger<ConnectedClientsService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ChargeManager _chargeManager;
    private readonly ILoggerFactory _loggerFactory;
    private readonly List<MqttMwBotClient> _connectedClients;

    public ConnectedClientsService(ILogger<ConnectedClientsService> logger, IServiceScopeFactory serviceScopeFactory, ChargeManager chargeManager, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _chargeManager = chargeManager;
        _loggerFactory = loggerFactory;
        _connectedClients = new List<MqttMwBotClient>();
    }

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
                _logger.LogDebug("Creating MqttMwBotClient for MwBot with id {id}", singleMwBot.Id);
                var client = new MqttMwBotClient(
                    _loggerFactory.CreateLogger<MqttMwBotClient>(),
                    _serviceScopeFactory,
                    _chargeManager);

                var connectResult = await client.InitializeAsync(singleMwBot.Id);
                if (!connectResult) // If connection fails, set MwBot status to offline
                {
                    _logger.LogError("Failed to connect MwBot with id {id} to MQTT server while getting connected clients", singleMwBot.Id);
                    singleMwBot.Status = MwBotStatus.Offline;
                    await mwBotRepository.UpdateAsync(singleMwBot);
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

    public List<MqttMwBotClient> GetConnectedClients()
    {
        return _connectedClients;
    }

    public void AddClient(MqttMwBotClient client)
    {
        _connectedClients.Add(client);
    }

    public void RemoveClient(MqttMwBotClient client)
    {
        _connectedClients.Remove(client);
    }
}
