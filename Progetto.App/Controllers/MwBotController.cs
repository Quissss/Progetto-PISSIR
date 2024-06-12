using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MQTTnet.Client;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Security;
using Progetto.App.Core.Services.MQTT;

namespace Progetto.App.Controllers;

/// <summary>
/// Controller for managing MwBots (endpoints for CRUD operations)
/// Requires admin authentication
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = PolicyNames.IsAdmin)]
public class MwBotController : ControllerBase
{
    private readonly ILogger<MwBotController> _logger;
    private readonly MwBotRepository _repository;
    private readonly ChargeHistoryRepository _chargeHistoryRepository;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly List<MqttMwBotClient> _connectedClients = new List<MqttMwBotClient>();

    public MwBotController(
        ILogger<MwBotController> logger,
        MwBotRepository repository,
        ChargeHistoryRepository chargeHistoryRepository,
        ILoggerFactory loggerFactory,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _repository = repository;
        _chargeHistoryRepository = chargeHistoryRepository;
        _loggerFactory = loggerFactory;
        _serviceScopeFactory = serviceScopeFactory;

        GetConnectedClients().GetAwaiter().GetResult();
    }

    private async Task GetConnectedClients()
    {
        _logger.LogDebug("Retrieving connected clients");
        try
        {
            var mwBotList = await _repository.GetOnlineMwBots();
            foreach (var singleMwBot in mwBotList)
            {
                _logger.LogDebug("Creating MqttMwBotClient for MwBot with id {id}", singleMwBot.Id);
                var client = new MqttMwBotClient(_loggerFactory.CreateLogger<MqttMwBotClient>(), _serviceScopeFactory);
                await client.InitializeAsync(singleMwBot.Id);
                _connectedClients.Add(client);
            }
        }
        catch
        {
            _logger.LogError("Error while retrieving online MwBots");
        }

        _logger.LogDebug("Connected clients retrieved");
    }

    [HttpPost]
    public async Task<ActionResult<MwBot>> AddMwBot([FromBody] MwBot mwBot)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while creating MwBot");
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Creating MwBot with id {id}", mwBot.Id);
            await _repository.AddAsync(mwBot);
            _logger.LogDebug("MwBot with id {id} created", mwBot.Id);

            return Ok(mwBot);
        }
        catch
        {
            _logger.LogError("Error while creating MwBot with id {id}", mwBot.Id);
        }

        return BadRequest();
    }

    [HttpPost("on")]
    public async Task<ActionResult<MqttMwBotClient>> TurnOn([FromBody] MwBot mwBot)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while creating MwBot");
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Creating / Retrieving MwBot with id {id}", mwBot.Id);
            var client = new MqttMwBotClient(_loggerFactory.CreateLogger<MqttMwBotClient>(), _serviceScopeFactory);
            await client.InitializeAsync(mwBot.Id);
            _connectedClients.Add(client);
            _logger.LogDebug("MwBot {id} initialized", mwBot.Id);

            return Ok(client);
        }
        catch
        {
            _logger.LogError("Error while creating MwBot with id {id}", mwBot.Id);
        }

        return BadRequest();
    }

    [HttpPost("off")]
    public async Task<ActionResult<MwBot>> TurnOff([FromBody] MwBot mwBot)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while turning off MwBot");
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Turning off MwBot with id {id}", mwBot.Id);
            var client = _connectedClients.FirstOrDefault(c => c.MwBot.Id == mwBot.Id);
            if (client != null)
            {
                _connectedClients.Remove(client);
            }
            _logger.LogDebug("MwBot with id {id} turned off", mwBot.Id);
            return Ok();
        }
        catch
        {
            _logger.LogError("Error while turning off MwBot with id {id}", mwBot.Id);
        }

        return BadRequest();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMwBot(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid id {id}", id);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Deleting MwBot with id {id}", id);
            await _repository.DeleteAsync(m => m.Id == id);
            _logger.LogDebug("MwBot with id {id} deleted", id);
            return Ok();
        }
        catch
        {
            _logger.LogError("Error while deleting MwBot with id {id}", id);
        }

        return BadRequest();
    }

    [HttpPut]
    public async Task<ActionResult<MwBot>> UpdateMwBot([FromBody] MwBot mwBot)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while updating MwBot with id {id}", mwBot.Id);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Updating MwBot with id {id}", mwBot.Id);
            await _repository.UpdateAsync(mwBot);
            _logger.LogDebug("MwBot with id {id} updated", mwBot.Id);
            return Ok(mwBot);
        }
        catch
        {
            _logger.LogError("Error while updating MwBot with id {id}", mwBot.Id);
        }

        return BadRequest();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MwBot>>> GetMwBots()
    {
        try
        {
            _logger.LogDebug("Retrieving MwBots");
            var mwBots = await _repository.GetAllAsync();
            _logger.LogDebug("MwBots retrieved");
            return Ok(mwBots);
        }
        catch
        {
            _logger.LogError("Error while retrieving MwBots");
        }

        return BadRequest();
    }
}
