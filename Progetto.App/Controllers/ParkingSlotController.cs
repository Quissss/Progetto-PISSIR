using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Security;
using Progetto.App.Core.Validators;

namespace Progetto.App.Controllers;

/// <summary>
/// Controller for parking slot operations (endpoints for CRUD operations)
/// Requires authentication
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ParkingSlotController : ControllerBase
{
    private readonly ILogger<ParkingSlotController> _logger;
    private readonly ParkingSlotRepository _parkingSlotRepository;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ParkingSlotController(ILogger<ParkingSlotController> logger, ParkingSlotRepository repository, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _parkingSlotRepository = repository;
        _serviceScopeFactory = serviceScopeFactory;
    }

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

            _logger.LogDebug("Parking slot created with {id}", parkingSlot.Id);
            return Ok(parkingSlot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating parking slot with id {id}", parkingSlot.Id);
        }

        return BadRequest();
    }

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

            _logger.LogDebug("Parking slot with id {id} deleted", parkingSlot.Id);
            return Ok(parkingSlot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting parking slot with id {id}", parkingSlot.Id);
        }

        return BadRequest();
    }

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

            var existingParkingSlot = await _parkingSlotRepository.GetByIdAsync(parkingSlot.Id);
            if (existingParkingSlot == null)
            {
                _logger.LogWarning("Parking slot with id {id} does not exist", parkingSlot.Id);
                return NotFound();
            }

            await _parkingSlotRepository.UpdateAsync(parkingSlot);

            _logger.LogDebug("Parking slot with id {id} updated", parkingSlot.Id);
            return Ok(parkingSlot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating parking slot with id {id}", parkingSlot.Id);
        }

        return BadRequest();
    }

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
