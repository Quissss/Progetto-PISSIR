using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;


namespace Progetto.App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ParkingSlotController : ControllerBase
    {
        private readonly ILogger<ParkingSlotController> _logger;
        private readonly ParkingSlotRepository _parkingSlotRepository;

        public ParkingSlotController(ILogger<ParkingSlotController> logger,ParkingSlotRepository parkingRepository)
        {
            _logger = logger;
            _parkingSlotRepository = parkingRepository;
        }

        // Method to get parking occupancy status
        [HttpGet("{parkingId}/occupancy")]
        public async Task<ActionResult<ParkingSlot>> GetParkingStatus(int parkingId)
        {
            try
            {
                var parkingSlot = await _parkingSlotRepository.GetByIdAsync(parkingId);
                if (parkingSlot == null)
                {
                    return NotFound("Parking slot not found.");
                }
                return Ok(parkingSlot.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving parking status.");
                return StatusCode(500, "Internal server error while retrieving parking status.");
            }
        }

        // Method to update parking slot occupancy status
        [HttpPut("slot/{slotId}/occupancy")]
        public async Task<ActionResult<ParkingSlot>> UpdateParkingSlotOccupancy(int slotId, [FromBody] ParkSlotStatus status)
        {
            try
            {
                var parkingSlot = await _parkingSlotRepository.GetByIdAsync(slotId);

                if (parkingSlot == null)
                {
                    return NotFound("Parking slot not found.");
                }

                parkingSlot.Status = status;
                await _parkingSlotRepository.UpdateAsync(parkingSlot);

                return Ok(parkingSlot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating parking slot occupancy.");
                return StatusCode(500, "Internal server error while updating parking slot occupancy.");
            }
        }

    }
}