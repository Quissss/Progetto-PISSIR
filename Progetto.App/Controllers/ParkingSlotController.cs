using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;


namespace Progetto.App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParkingSlotController : ControllerBase
    {
        private readonly ParkingSlotRepository _parkingSlotRepository;

        public ParkingSlotController(ParkingSlotRepository parkingRepository)
        {
            _parkingSlotRepository = parkingRepository;
        }

        // Metodo per ottenere lo stato di occupazione del parcheggio
        [HttpGet("{parkingId}/occupancy")]
        public async Task<ActionResult<ParkingSlot>> GetParkingStatus(int parkingId)
        {
            var parkSingle = await _parkingSlotRepository.GetByIdAsync(parkingId);
            if (parkSingle == null)
            {
                return NotFound();
            }
            return Ok(parkSingle.Status);
        }

        // Metodo per aggiornare lo stato di occupazione di uno slot di parcheggio
        [HttpPut("slot/{slotId}/occupancy")]
        public async Task<ActionResult<ParkingSlot>> UpdateParkingSlotOccupancy(int slotId, [FromBody] ParkSlotStatus status)
        {
            var parkSingle = await _parkingSlotRepository.GetByIdAsync(slotId);
            parkSingle.Status = status;
            await _parkingSlotRepository.UpdateAsync(parkSingle);

            if (parkSingle == null)
            {
                return NotFound();
            }
            return Ok(parkSingle);
        }

    }
}