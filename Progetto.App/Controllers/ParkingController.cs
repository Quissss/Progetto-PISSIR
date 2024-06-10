using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Security;
using System.Linq.Expressions;

namespace Progetto.App.Controllers;

[Route("api/[controller]")]
[ApiController]
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
    [Authorize(Policy = PolicyNames.IsAdmin)]
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

    [HttpDelete]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult> DeleteParking([FromBody] Parking parking)
    {
        if (parking.Id <= 0)
        {
            _logger.LogWarning("Invalid id {id}", parking.Id);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Deleting parking with id {id}", parking.Id);

            await _parkingRepository.DeleteAsync(p => p.Id == parking.Id);

            _logger.LogDebug("Parking with id {id} deleted", parking.Id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting parking with id {id}", parking.Id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult> UpdateParking([FromBody] Parking parking)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while updating parking with name {name}", parking.Name);
            return BadRequest();
        }

        try
        {
            _logger.LogDebug("Updating parking with id {id}", parking.Id);

            var existingParking = await _parkingRepository.GetByIdAsync(parking.Id);
            if (existingParking == null)
            {
                _logger.LogWarning("Parking with id {id} not found", parking.Id);
                return NotFound();
            }

            await _parkingRepository.UpdateAsync(parking);

            _logger.LogDebug("Parking with id {id} updated", parking.Id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating parking with id {id}", parking.Id);
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
