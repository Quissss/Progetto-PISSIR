using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Security;
using Progetto.App.Core.Services.SignalR.Hubs;
using Progetto.App.Core.Validators;

namespace Progetto.App.Controllers;

/// <summary>
/// Controller for parking slot operations (endpoints for CRUD operations)
/// Requires authentication
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ParkingSlotController : ControllerBase
{
    private readonly IHubContext<ParkingSlotHub> _hubContext;
    private readonly ILogger<ParkingSlotController> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ParkingSlotRepository _parkingSlotRepository;

    /// <summary>
    /// Controller per la gestione delle operazioni sui posti auto (operazioni CRUD).
    /// Richiede autenticazione.
    /// </summary>
    /// <param name="logger">Interfaccia per il logging</param>
    /// <param name="hubContext">Contesto SignalR per notificare i client connessi</param>
    /// <param name="repository">Repository per la gestione dei dati sui posti auto</param>
    /// <param name="serviceScopeFactory">Factory per la gestione degli scope dei servizi</param>
    public ParkingSlotController(IHubContext<ParkingSlotHub> hubContext, ILogger<ParkingSlotController> logger, ParkingSlotRepository repository, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _hubContext = hubContext;
        _parkingSlotRepository = repository;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// Aggiunge un nuovo posto auto
    /// </summary>
    /// <param name="parkingSlot">Dati del posto auto da aggiungere</param>
    /// <returns>Il posto auto creato o un errore in caso di fallimento</returns>
    [HttpPost]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<ParkingSlot>> AddParkingSlot([FromBody] ParkingSlot parkingSlot)
    {
        var validator = new ParkingSlotValidator();
        var result = validator.Validate(parkingSlot);
        result.AddToModelState(ModelState);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while creating parking slot with id {id}", parkingSlot.Id);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Creating parking slot with id {id}", parkingSlot.Id);

            var existingParkingSlot = await _parkingSlotRepository.GetByIdAsync(parkingSlot.Id);
            if (existingParkingSlot != null)
            {
                _logger.LogWarning("Parking slot with id {id} already exists", parkingSlot.Id);
                return BadRequest();
            }

            await _parkingSlotRepository.AddAsync(parkingSlot);
            var connectionId = Request.Headers["X-Connection-Id"].ToString();
            await _hubContext.Clients.AllExcept(connectionId).SendAsync("ParkingSlotAdded", parkingSlot);

            _logger.LogDebug("Parking slot created with {id}", parkingSlot.Id);
            return Ok(parkingSlot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating parking slot with id {id}", parkingSlot.Id);
        }

        return BadRequest();
    }

    /// <summary>
    /// Elimina un posto auto esistente
    /// </summary>
    /// <param name="parkingSlot">Dati del posto auto da eliminare</param>
    /// <returns>Conferma dell'eliminazione o un errore in caso di fallimento</returns>
    [HttpDelete]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult> DeleteParkingSlot([FromBody] ParkingSlot parkingSlot)
    {
        if (parkingSlot.Id <= 0)
        {
            _logger.LogWarning("Invalid id while deleting parking slot with id {id}", parkingSlot.Id);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Deleting parking slot with id {id}", parkingSlot.Id);

            var existingParkingSlot = await _parkingSlotRepository.GetByIdAsync(parkingSlot.Id);
            if (existingParkingSlot == null)
            {
                _logger.LogWarning("Parking slot with id {id} does not exist", parkingSlot.Id);
                return NotFound();
            }

            await _parkingSlotRepository.DeleteAsync(p => p.Id == parkingSlot.Id);
            var connectionId = Request.Headers["X-Connection-Id"].ToString();
            await _hubContext.Clients.AllExcept(connectionId).SendAsync("ParkingSlotDeleted", parkingSlot.Id);

            _logger.LogDebug("Parking slot with id {id} deleted", parkingSlot.Id);
            return Ok(parkingSlot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting parking slot with id {id}", parkingSlot.Id);
        }

        return BadRequest();
    }

    /// <summary>
    /// Aggiorna i dati di un posto auto esistente
    /// </summary>
    /// <param name="parkingSlot">Dati aggiornati del posto auto</param>
    /// <returns>Il posto auto aggiornato o un errore in caso di fallimento</returns>
    [HttpPut]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<ParkingSlot>> UpdateParkingSlot([FromBody] ParkingSlot parkingSlot)
    {
        var validator = new ParkingSlotValidator();
        var result = validator.Validate(parkingSlot);
        result.AddToModelState(ModelState);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while updating parking slot with id {id}", parkingSlot.Id);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Updating parking slot with id {id}", parkingSlot.Id);

            if (!await _parkingSlotRepository.CheckEntityExists(parkingSlot))
            {
                _logger.LogWarning("Parking slot with id {id} does not exist", parkingSlot.Id);
                return NotFound();
            }

            await _parkingSlotRepository.UpdateAsync(parkingSlot);
            var connectionId = Request.Headers["X-Connection-Id"].ToString();
            await _hubContext.Clients.AllExcept(connectionId).SendAsync("ParkingSlotUpdated", parkingSlot);

            _logger.LogDebug("Parking slot with id {id} updated", parkingSlot.Id);
            return Ok(parkingSlot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating parking slot with id {id}", parkingSlot.Id);
        }

        return BadRequest();
    }

    /// <summary>
    /// Ottiene una lista di tutti i posti auto con opzioni di filtraggio per numero, id del parcheggio o stato
    /// </summary>
    /// <param name="number">Numero del posto auto</param>
    /// <param name="parkingId">ID del parcheggio associato</param>
    /// <param name="status">Stato del posto auto</param>
    /// <returns>La lista dei posti auto filtrati o un errore in caso di fallimento</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ParkingSlot>>> GetParkingSlots([FromQuery] int? number, int? parkingId, ParkingSlotStatus? status)
    {
        try
        {
            _logger.LogDebug("Getting all parking slots");

            var parkingSlots = await _parkingSlotRepository.GetAllAsync();

            var scope = _serviceScopeFactory.CreateScope();
            var parkingRepository = scope.ServiceProvider.GetRequiredService<ParkingRepository>();

            var parkings = await parkingRepository.GetAllAsync();
            parkingSlots.ForEach(p => p.Parking = parkings.FirstOrDefault(parking => parking.Id == p.ParkingId));

            if (parkingSlots == null)
            {
                _logger.LogWarning("No parking slot found");
                return NotFound();
            }

            if (number is not null)
            {
                parkingSlots = parkingSlots.Where(p => p.Number == number).ToList();
            }
            else if (parkingId is not null)
            {
                parkingSlots = parkingSlots.Where(p => p.ParkingId == parkingId).ToList();
            }
            if (status is not null)
            {
                parkingSlots = parkingSlots.Where(p => p.Status == status).ToList();
            }

            _logger.LogDebug("Returning {count} parking slots", parkingSlots.Count());

            return Ok(parkingSlots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting all parking slots");
        }

        return BadRequest();
    }

    /// <summary>
    /// Ottiene i dettagli di un posto auto tramite l'id
    /// </summary>
    /// <param name="id">ID del posto auto da recuperare</param>
    /// <returns>Il posto auto corrispondente o un errore se non trovato</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ParkingSlot>> GetParkingSlot(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid id while getting parking slot with id {id}", id);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Getting parking slot with id {id}", id);

            var parkingSlot = await _parkingSlotRepository.GetByIdAsync(id);
            if (parkingSlot == null)
            {
                _logger.LogWarning("Parking slot with id {id} not found", id);
                return NotFound();
            }

            _logger.LogDebug("Returning parking slot with id {id}", id);
            return Ok(parkingSlot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting parking slot with id {id}", id);
        }

        return BadRequest();
    }

    /// <summary>
    /// Ottiene tutti gli stati possibili per i posti auto
    /// </summary>
    /// <returns>La lista degli stati dei posti auto</returns>
    [HttpGet("statuses")]
    public IActionResult GetStatuses()
    {
        var statuses = Enum.GetValues(typeof(ParkingSlotStatus))
                           .Cast<ParkingSlotStatus>()
                           .Select(e => new { Id = (int)e, Name = e.ToString() })
                           .ToList();

        return Ok(statuses);
    }
}
