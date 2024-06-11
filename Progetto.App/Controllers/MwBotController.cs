using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MQTTnet.Client;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Security;
using Progetto.App.Core.Services.MQTT;

namespace Progetto.App.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = PolicyNames.IsAdmin)]
public class MwBotController : ControllerBase
{
    private readonly ILogger<MwBotController> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly MwBotRepository _repository;
    private readonly ChargeHistoryRepository _chargeHistoryRepository;



    private List<MqttMwBotClient> _connectedClients;

    public MwBotController(ILogger<MwBotController> logger, MwBotRepository repository, ChargeHistoryRepository chargeHistoryRepository, ILoggerFactory loggerFactory, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _serviceScopeFactory = serviceScopeFactory;
        _repository = repository;
        _chargeHistoryRepository = chargeHistoryRepository;

        GetConnectedClients().GetAwaiter();
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
                _connectedClients.Add(new MqttMwBotClient(_loggerFactory.CreateLogger<MqttMwBotClient>(), _serviceScopeFactory));
                await _connectedClients.Last().InitializeAsync(singleMwBot.Id);
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
    public async Task<ActionResult<MwBot>> TurnOn([FromBody] MwBot mwBot)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while creating MwBot");
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Creating / Retrieving MwBot with id {id}", mwBot.Id);
            _connectedClients.Add(new MqttMwBotClient(_loggerFactory.CreateLogger<MqttMwBotClient>(), _serviceScopeFactory));
            _logger.LogDebug("MwBot {id} initialized", mwBot.Id);

            return Ok(_connectedClients.Last());
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
            // TODO : Implement turning off MwBot
            _connectedClients.Remove(_connectedClients.Find(mc => mc.MwBot.Id == mwBot.Id));
        }
        catch
        {

        }

        return BadRequest();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteMwBot([FromBody] MwBot mwBot)
    {
        if (mwBot.Id <= 0)
        {
            _logger.LogWarning("Invalid id {id}", mwBot.Id);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Deleting MwBot with id {id}", mwBot.Id);
            await _repository.DeleteAsync(m => m.Id == mwBot.Id);
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
