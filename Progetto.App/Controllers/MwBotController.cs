using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ChargeManager _chargeManager;
    private readonly List<MqttMwBotClient> _connectedClients;

    public MwBotController(
        ILogger<MwBotController> logger,
        MwBotRepository repository,
        ILoggerFactory loggerFactory,
        IServiceScopeFactory serviceScopeFactory,
        ChargeManager chargeManager)
    {
        _logger = logger;
        _repository = repository;
        _loggerFactory = loggerFactory;
        _serviceScopeFactory = serviceScopeFactory;
        _chargeManager = chargeManager;
        _connectedClients = new List<MqttMwBotClient>();

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
                var client = new MqttMwBotClient(
                        _loggerFactory.CreateLogger<MqttMwBotClient>(),
                        _serviceScopeFactory,
                        _chargeManager);

                var connectResult = await client.InitializeAsync(singleMwBot.Id);
                if (!connectResult) // If connection fails, set MwBot status to offline
                {
                    _logger.LogError("Failed to connect MwBot with id {id} to MQTT server while getting connected clients", singleMwBot.Id);
                    singleMwBot.Status = MwBotStatus.Offline;
                    _ = _repository.UpdateAsync(singleMwBot);
                }
                else
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

    [HttpPut("on")]
    public async Task<ActionResult<MwBot>> TurnOn([FromBody] MwBot mwBot)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while creating MwBot");
            return BadRequest();
        }

        if (mwBot is null)
        {
            _logger.LogWarning("MwBot not set, cannot turn on");
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Creating / Retrieving MwBot with id {id}", mwBot.Id);
            var client = new MqttMwBotClient(
                _loggerFactory.CreateLogger<MqttMwBotClient>(),
                _serviceScopeFactory,
                _chargeManager);

            if (client is null)
            {
                _logger.LogWarning("Client not initialized, cannot turn on");
                return BadRequest();
            }

            var connectResult = await client.InitializeAsync(mwBot.Id);
            if (!connectResult)
            {
                _logger.LogError("Failed to connect MwBot with id {id} to MQTT server while turning on", mwBot.Id);
                return BadRequest();
            }

            client.mwBot.Status = mwBot.Status = MwBotStatus.StandBy;
            await _repository.UpdateAsync(mwBot);
            _connectedClients.Add(client);

            _logger.LogDebug("MwBot {id} initialized", mwBot.Id);
            return Ok(mwBot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating MwBot with id {id}", mwBot.Id);
            return BadRequest();
        }
    }

    [HttpPut("off")]
    public async Task<ActionResult<MwBot>> TurnOff([FromBody] MwBot mwBot)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while turning off MwBot");
            return BadRequest();
        }

        if (mwBot is null)
        {
            _logger.LogWarning("MwBot not initialized, cannot turn off");
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Turning off MwBot with id {id}", mwBot.Id);
            var client = _connectedClients.FirstOrDefault(c => c.mwBot?.Id == mwBot.Id);

            if (client != null)
            {
                await client.DisconnectAsync();
                _connectedClients.Remove(client);
            }

            mwBot.Status = MwBotStatus.Offline;
            _ = _repository.UpdateAsync(mwBot);

            _logger.LogDebug("MwBot with id {id} turned off", mwBot.Id);
            return Ok(mwBot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while turning off MwBot with id {id}", mwBot.Id);
            return BadRequest();
        }
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
    public async Task<ActionResult<IEnumerable<MwBot>>> GetMwBots([FromQuery] int? status)
    {
        try
        {
            _logger.LogDebug("Retrieving MwBots");

            IEnumerable<MwBot> mwBots;

            if (status.HasValue && status.Value == -1)
            {
                mwBots = await _repository.GetOnlineMwBots();
            }
            else if (status.HasValue && status.Value == 0)
            {
                mwBots = await _repository.GetOfflineMwBots();
            }
            else
            {
                mwBots = await _repository.GetAllAsync();
            }

            _logger.LogDebug("MwBots retrieved");
            return Ok(mwBots);
        }
        catch
        {
            _logger.LogError("Error while retrieving MwBots");
        }

        return BadRequest();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MwBot>> GetMwBot(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid id {id}", id);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Retrieving MwBot with id {id}", id);
            var mwBot = await _repository.GetByIdAsync(id);

            if (mwBot == null)
            {
                _logger.LogWarning("MwBot with id {id} not found", id);
                return NotFound();
            }

            _logger.LogDebug("MwBot with id {id} retrieved", id);
            return Ok(mwBot);
        }
        catch
        {
            _logger.LogError("Error while retrieving MwBot with id {id}", id);
        }

        return BadRequest();
    }

    //[HttpGet("chargelevel/{id}")]
}
