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
/// Controller for managing parkings (endpoints for CRUD operations)
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ParkingController : ControllerBase
{
    private readonly ILogger<ParkingController> _logger;
    private readonly IHubContext<ParkingHub> _hubContext;
    private readonly ParkingRepository _parkingRepository;

    public ParkingController(ILogger<ParkingController> logger, IHubContext<ParkingHub> hubContext, ParkingRepository repository)
    {
        _logger = logger;
        _hubContext = hubContext;
        _parkingRepository = repository;
    }

    [HttpPost]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<Parking>> AddParking([FromBody] Parking parking)
    {
        var validator = new ParkingValidator();
        var result = validator.Validate(parking);
        result.AddToModelState(ModelState);

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
            var connectionId = Request.Headers["X-Connection-Id"].ToString();
            await _hubContext.Clients.AllExcept(connectionId).SendAsync("ParkingAdded", parking);

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
            var connectionId = Request.Headers["X-Connection-Id"].ToString();
            await _hubContext.Clients.AllExcept(connectionId).SendAsync("ParkingDeleted", parking.Id);

            _logger.LogDebug("Parking with id {id} deleted", parking.Id);
            return Ok(parking);
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
        var validator = new ParkingValidator();
        var result = validator.Validate(parking);
        result.AddToModelState(ModelState);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while updating parking with name {name}", parking.Name);
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogDebug("Updating parking with id {id}", parking.Id);

            if (!(await _parkingRepository.CheckEntityExists(parking)))
            {
                _logger.LogWarning("Parking with id {id} not found", parking.Id);
                return NotFound(parking);
            }

            await _parkingRepository.UpdateAsync(parking);
            var connectionId = Request.Headers["X-Connection-Id"].ToString();
            await _hubContext.Clients.AllExcept(connectionId).SendAsync("ParkingUpdated", parking);

            _logger.LogDebug("Parking with id {id} updated", parking.Id);
            return Ok(parking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating parking with id {id}", parking.Id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Parking>>> GetAllParkings([FromQuery] string? name, [FromQuery] string? city, [FromQuery] string? address)
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

            if (name is not null)
                parkings = parkings.Where(p => p.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase)).ToList();
            else if (city is not null)
                parkings = parkings.Where(p => p.City.Contains(city, StringComparison.InvariantCultureIgnoreCase)).ToList();
            else if (address is not null)
                parkings = parkings.Where(p => p.Address.Contains(address, StringComparison.InvariantCultureIgnoreCase)).ToList();
            
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
