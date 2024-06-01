using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Security;
using System.Diagnostics.Metrics;
using System.Net;
using System.Xml.Linq;

namespace Progetto.App.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ParkingController : ControllerBase
{
    private readonly ILogger<ParkingController> _logger;
    private readonly ParkingRepository _parkingRepository;

    public ParkingController(ILogger<ParkingController> logger,ParkingRepository parkingRepository)
    {
        _logger = logger;
        _parkingRepository = parkingRepository;
    }

    // Method for creating a parking
    [HttpPost("/create")]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult<Parking>> AddParking([FromForm] Parking park  )
    {

        try
        {
            var createdParkingSlot = await _parkingRepository.AddAsync(park);
            return Ok(createdParkingSlot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while adding parking.");
            return StatusCode(500, "Internal server error while adding parking.");
        }
    }

    // Method for deleting a parking
    [HttpDelete("/delete/{parkId}")]
    [Authorize(Policy = PolicyNames.IsAdmin)]
    public async Task<ActionResult> DeleteParking(int parkId)
    {

        try
        {
            await _parkingRepository.DeleteAsync(p => p.Id == parkId);
            await _parkingRepository.SaveAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting parking.");
            return StatusCode(500, "Internal server error while deleting parking.");
        }
    }

    // Method to get all the parking
    [HttpGet("/listParking")]
    public async Task<ActionResult<Parking>> GetAllParking()
    {
        try
        {
            var allParking = await _parkingRepository.GetAllAsync();
            return Ok(allParking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while retrieving all parking.");
            return StatusCode(500, "Internal server error while retrieving all parking.");
        }
    }



}
