
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Security;
using Progetto.App.Core.Services.Mqtt;
using Progetto.App.Core.Services.SignalR.Hubs;
using Progetto.App.Core.Validators;

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
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<MwBotController> _logger;
    private readonly IHubContext<MwBotHub> _hubContext;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly MwBotRepository _mwBotRepository;
    private readonly ConnectedClientsService _connectedClientsService;

    public MwBotController(
        ILogger<MwBotController> logger,
        MwBotRepository mwBotRespository,
        IHubContext<MwBotHub> hubContext,
        ILoggerFactory loggerFactory,
        IServiceScopeFactory serviceScopeFactory,
        ConnectedClientsService connectedClientsService)
    {
        _logger = logger;
        _hubContext = hubContext;
        _loggerFactory = loggerFactory;
        _serviceScopeFactory = serviceScopeFactory;
        _connectedClientsService = connectedClientsService;

        var provider = serviceScopeFactory.CreateScope().ServiceProvider;
        _mwBotRepository = provider.GetRequiredService<MwBotRepository>();
    }

    [HttpPost]
    public async Task<ActionResult<MwBot>> AddMwBot([FromBody] MwBot mwBot)
    {
        var validator = new MwBotValidator();
        var result = validator.Validate(mwBot);
        result.AddToModelState(ModelState);


        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while creating MwBot");
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Creating MwBot with id {id}", mwBot.Id);

            await _mwBotRepository.AddAsync(mwBot);
            var connectionId = Request.Headers["X-Connection-Id"].ToString();
            await _hubContext.Clients.AllExcept(connectionId).SendAsync("MwBotAdded", mwBot);

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

        var validator = new MwBotValidator();
        var result = validator.Validate(mwBot);
        result.AddToModelState(ModelState);

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

            var existingClient = _connectedClientsService.GetConnectedClients().FirstOrDefault(c => c.MwBot?.Id == mwBot.Id);

            if (existingClient != null)
            {
                _logger.LogWarning("MwBot {id} is already turned on", mwBot.Id);
                return BadRequest("MwBot is already turned on");
            }

            mwBot.Status = MwBotStatus.StandBy;
            await _mwBotRepository.UpdateAsync(mwBot);
            var connectionId = Request.Headers["X-Connection-Id"].ToString();
            await _hubContext.Clients.AllExcept(connectionId).SendAsync("MwBotUpdated", mwBot);

            var client = new MqttMwBotClient(
                _loggerFactory.CreateLogger<MqttMwBotClient>(),
                _serviceScopeFactory);

            if (client is null)
            {
                _logger.LogWarning("Client not initialized, cannot turn on");
                return BadRequest();
            }

            client.MwBot ??= mwBot;

            var connectResult = await client.InitializeAsync(mwBot.Id);
            if (!connectResult)
            {
                _logger.LogError("Failed to connect MwBot with id {id} to MQTT server while turning on", mwBot.Id);
                return BadRequest();
            }

            _connectedClientsService.AddClient(client);

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
        var validator = new MwBotValidator();
        var result = validator.Validate(mwBot);
        result.AddToModelState(ModelState);

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
            // TODO: togliere l'affidamento della ricarica al bot spento (rimuovere mwbotid da currentlycharging e riassegnarlo quando un bot trova la ricarica non completata)

            _logger.LogDebug("Turning off MwBot with id {id}", mwBot.Id);
            var client = _connectedClientsService.GetConnectedClients().FirstOrDefault(c => c.MwBot?.Id == mwBot.Id);

            if (client != null)
            {
                await client.DisconnectAsync();
                _connectedClientsService.RemoveClient(client);
                client.Dispose();
            }

            mwBot.Status = MwBotStatus.Offline;
            await _mwBotRepository.UpdateAsync(mwBot);
            var connectionId = Request.Headers["X-Connection-Id"].ToString();
            await _hubContext.Clients.AllExcept(connectionId).SendAsync("MwBotUpdated", mwBot);

            _logger.LogDebug("MwBot with id {id} turned off", mwBot.Id);
            return Ok(mwBot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while turning off MwBot with id {id}", mwBot.Id);
            return BadRequest();
        }
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteMwBot([FromBody] MwBot mwBot)
    {
        try
        {
            _logger.LogDebug("Deleting MwBot with id {id}", mwBot.Id);

            await _mwBotRepository.DeleteAsync(m => m.Id == mwBot.Id);
            var connectionId = Request.Headers["X-Connection-Id"].ToString();
            await _hubContext.Clients.AllExcept(connectionId).SendAsync("MwBotDeleted", mwBot.Id);

            _logger.LogDebug("MwBot with id {id} deleted", mwBot.Id);
            return Ok();
        }
        catch
        {
            _logger.LogError("Error while deleting MwBot with id {id}", mwBot.Id);
        }

        return BadRequest();
    }

    [HttpPut]
    public async Task<ActionResult<MwBot>> UpdateMwBot([FromBody] MwBot mwBot)
    {
        var validator = new MwBotValidator();
        var result = validator.Validate(mwBot);
        result.AddToModelState(ModelState);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while updating MwBot with id {id}", mwBot.Id);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Updating MwBot with id {id}", mwBot.Id);

            await _mwBotRepository.UpdateAsync(mwBot);
            var connectionId = Request.Headers["X-Connection-Id"].ToString();
            await _hubContext.Clients.AllExcept(connectionId).SendAsync("MwBotUpdated", mwBot);

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
    public async Task<ActionResult<IEnumerable<MwBot>>> GetMwBots([FromQuery] int? status, [FromQuery] int? parkingId)
    {
        try
        {
            _logger.BeginScope("Retrieving MwBots");

            IEnumerable<MwBot> mwBots = await _mwBotRepository.GetAllAsync();

            if (status.HasValue && status.Value == 1)
                mwBots = mwBots.Where(p => p.Status != MwBotStatus.Offline);
            else if (status.HasValue && status.Value == 0)
                mwBots = mwBots.Where(p => p.Status == MwBotStatus.Offline);

            if (parkingId.HasValue && parkingId.Value > -1)
            {
                mwBots = mwBots.Where(b => b.ParkingId == parkingId.Value).ToList();
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
            var mwBot = await _mwBotRepository.GetByIdAsync(id);

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
