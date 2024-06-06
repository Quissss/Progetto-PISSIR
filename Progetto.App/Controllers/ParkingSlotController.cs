using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Security;

namespace Progetto.App.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ParkingSlotController : ControllerBase
{
    private readonly ILogger<ParkingSlotController> _logger;
    private readonly ParkingSlotRepository _parkingSlotRepository;

    public ParkingSlotController(ILogger<ParkingSlotController> logger, ParkingSlotRepository repository)
    {
        _logger = logger;
        _parkingSlotRepository = repository;
    }

    [HttpPost]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<ParkingSlot>> AddParkingSlot([FromBody] ParkingSlot parkingSlot)
    {
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
            await _parkingSlotRepository.SaveAsync();

            _logger.LogDebug("Parking slot created with {id}", parkingSlot.Id);
            return Ok(existingParkingSlot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating parking slot with id {id}", parkingSlot.Id);
        }

        return BadRequest();
    }

    [HttpDelete]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult> DeleteParkingSlot([FromBody] int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid id while deleting parking slot with id {id}", id);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Deleting parking slot with id {id}", id);

            var existingParkingSlot = await _parkingSlotRepository.GetByIdAsync(id);
            if (existingParkingSlot == null)
            {
                _logger.LogWarning("Parking slot with id {id} does not exist", id);
                return NotFound();
            }

            await _parkingSlotRepository.DeleteAsync(p => p.Id == id);
            await _parkingSlotRepository.SaveAsync();

            _logger.LogDebug("Parking slot with id {id} deleted", id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting parking slot with id {id}", id);
        }

        return BadRequest();
    }

    [HttpPut]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<ParkingSlot>> UpdateParkingSlot([FromBody] ParkingSlot parkingSlot)
    {
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
            await _parkingSlotRepository.SaveAsync();

            _logger.LogDebug("Parking slot with id {id} updated", parkingSlot.Id);
            return Ok(existingParkingSlot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating parking slot with id {id}", parkingSlot.Id);
        }

        return BadRequest();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ParkingSlot>>> GetParkingSlots()
    {
        try
        {
            _logger.LogDebug("Getting all parking slots");

            var parkingSlots = await _parkingSlotRepository.GetAllAsync();
            _logger.LogDebug("Returning {count} parking slots", parkingSlots.Count());

            return Ok(parkingSlots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting all parking slots");
        }

        return BadRequest();
    }

    [HttpGet("parking-slot/{id}")]
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

    [HttpGet("parking-slot/name/{name}")]
    public async Task<ActionResult<ParkingSlot>> GetParkingSlotByName(int name)
    {
        if (name <= 0)
        {
            _logger.LogWarning("Invalid name while getting parking slot with name {name}", name);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Getting parking slot with name {name}", name);

            var parkingSlot = await _parkingSlotRepository.GetByIdAsync(name);
            if (parkingSlot == null)
            {
                _logger.LogWarning("Parking slot with name {name} not found", name);
                return NotFound();
            }

            _logger.LogDebug("Returning parking slot {name}", name);
            return Ok(parkingSlot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting parking slot with name {name}", name);
        }

        return BadRequest();
    }

    [HttpGet("free-parking-slots")]
    public async Task<ActionResult<IEnumerable<ParkingSlot>>> GetFreeParkingSlots()
    {
        try
        {
            _logger.LogDebug("Getting all free parking slots");

            var parkingSlots = await _parkingSlotRepository.GetFreeParkingSlots();
            _logger.LogDebug("Returning {count} free parking slots", parkingSlots.Count());

            return Ok(parkingSlots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting all free parking slots");
        }

        return BadRequest();
    }

    [HttpGet("occupied-parking-slots")]
    public async Task<ActionResult<IEnumerable<ParkingSlot>>> GetOccupiedParkingSlots()
    {
        try
        {
            _logger.LogDebug("Getting all occupied parking slots");

            var parkingSlots = await _parkingSlotRepository.GetOccupiedParkingSlots();
            _logger.LogDebug("Returning {count} occupied parking slots", parkingSlots.Count());

            return Ok(parkingSlots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting all occupied parking slots");
        }

        return BadRequest();
    }
}
