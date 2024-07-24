using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CamSimulatorController : ControllerBase
    {

        readonly private CarRepository _carRepository;
        readonly private ILogger<CamSimulatorController> _logger;

        public CamSimulatorController(CarRepository carRepository, ILogger<CamSimulatorController> logger)
        {
            _carRepository = carRepository;
            _logger = logger;

        }

        [HttpPost("arrival")]
        public async Task<IActionResult> Entrata([FromBody] Request request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.LicensePlate))
            {
                return BadRequest("Targa non valida");
            }

            // Simulazione dell'elaborazione della targa
            string responseMessage = $"Targa ricevuta: {request.LicensePlate}, ParkingId ricevuto: {request.ParkingId}";

            var car = await _carRepository.UpdateCarStatus(request.LicensePlate, CarStatus.Waiting);

            return Ok(responseMessage);
        }
    }

    public class Request
    {
        public string LicensePlate { get; set; }
        public int ParkingId {  get; set; } 
    }
}