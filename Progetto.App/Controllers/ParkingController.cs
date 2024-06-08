using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Security;
using System.Linq.Expressions;

namespace Progetto.App.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = PolicyNames.IsAdmin)]
public class ParkingController : ControllerBase
{
    private readonly ILogger<ParkingController> _logger;
    private readonly ParkingRepository _parkingRepository;

    public ParkingController(ILogger<ParkingController> logger, ParkingRepository repository)
    {
        _logger = logger;
        _parkingRepository = repository;
    }

    [HttpPost]
    public async Task<ActionResult<Parking>> AddParking([FromBody] Parking parking)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while creating parking with name {name}", parking.Name);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Creating parking with name {name}", parking.Name);

            var existingParking = await _parkingRepository.GetParkingByName(parking.Name);
            if (existingParking != null)
            {
                _logger.LogWarning("Parking with name {name} already exists", parking.Name);
                return BadRequest();
            }

            await _parkingRepository.AddAsync(parking);

            _logger.LogDebug("Parking with {name} created", parking.Name);
            return Ok(parking); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating parking with name {name}", parking.Name);
        }

        return BadRequest();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteParking(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid id {id}", id);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Deleting parking with id {id}", id);

            await _parkingRepository.DeleteAsync(p => p.Id == id);

            _logger.LogDebug("Parking with id {id} deleted", id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting parking with id {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateParking(int id, [FromBody] Parking parking)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while updating parking with name {name}", parking.Name);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Updating parking with id {id}", id);

            var existingParking = await _parkingRepository.GetByIdAsync(id);
            if (existingParking == null)
            {
                _logger.LogWarning("Parking with id {id} not found", id);
                return NotFound();
            }

            await _parkingRepository.UpdateAsync(parking);

            _logger.LogDebug("Parking with id {id} updated", id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating parking with id {id}", id);
        }

        return BadRequest();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Parking>>> GetAllParkings()
    {
        try
        {
            _logger.LogDebug("Getting all parkings");

            var parkings = await _parkingRepository.GetAllAsync();
            if (parkings == null)
            {
                _logger.LogWarning("No parkings found");
                return NotFound();
            }

            _logger.LogDebug("Returning {count} parkings", parkings.Count());
            return Ok(parkings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting all parkings");
        }

        return BadRequest();
    }

    [HttpGet("{name}")]
    public async Task<ActionResult<Parking>> GetParkingByName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogWarning("Invalid name {name}", name);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Getting parking {name}", name);

            var parking = await _parkingRepository.GetParkingByName(name);
            if (parking == null)
            {
                _logger.LogWarning("No parkings found with name {name}", name);
                return NotFound();
            }

            _logger.LogDebug("Returning parking {name}", name);
            return Ok(parking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting parking {name}", name);
        }

        return BadRequest();
    }
}
