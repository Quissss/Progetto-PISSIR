using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CamSimulatorController : ControllerBase
{
    private readonly CarRepository _carRepository;
    private readonly ReservationRepository _reservationRepository;
    private readonly ParkingSlotRepository _parkingSlotRepository;
    private readonly StopoverRepository _stopoverRepository;
    private ChargeManager _chargeManager { get; set; }
    private readonly ILogger<CamSimulatorController> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly UserManager<IdentityUser> _userManager;

    public CamSimulatorController(
        CarRepository carRepository,
        ILogger<CamSimulatorController> logger,
        IServiceScopeFactory serviceScopeFactory,
        ReservationRepository reservationRepository,
        ChargeManager chargeManager,
        ParkingSlotRepository parkingSlotRepository,
        StopoverRepository stopoverRepository,
        UserManager<IdentityUser> userManager)
    {
        _carRepository = carRepository;
        _reservationRepository = reservationRepository;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _chargeManager = chargeManager;
        _parkingSlotRepository = parkingSlotRepository;
        _userManager = userManager;
        _stopoverRepository = stopoverRepository;
    }

    [HttpPost("detect")]
    public async Task<IActionResult> Detect([FromBody] Request request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.LicencePlate))
        {
            return BadRequest("Targa non valida");
        }

        // Simulazione dell'elaborazione della targa
        string responseMessage = $"Targa rilevata: {request.LicencePlate} sul parcheggio n.{request.ParkingId}";
        var car = await _carRepository.GetCarByLicencePlate(request.LicencePlate);

        if (car == null)
        {
            _logger.LogInformation("Carplate {plate} not found", request.LicencePlate);
            return NotFound(responseMessage);
        }

        if (car.Status == CarStatus.OutOfParking)
        {
            await CarEntering(request, car);
        }
        else
        {
            await CarDeparting(request, car);
        }

        return Ok(responseMessage);
    }

    private async Task CarEntering(Request request, Car car)
    {
        _logger.LogInformation("Detected carplate {plate} on arrival", request.LicencePlate);

        car.Status = CarStatus.Waiting;
        car.ParkingId = request.ParkingId;
        await _carRepository.UpdateAsync(car);
    }

    private async Task CarDeparting(Request request, Car car)
    {   
        if (car.ParkingId != request.ParkingId)
        {
            _logger.LogWarning("Carplate {plate} is not in parking {parkingId}", request.LicencePlate, request.ParkingId);
            return;
        }

        _logger.LogInformation("Detected carplate {plate} on departure", request.LicencePlate);

        // Remove related requests
        var scope = _serviceScopeFactory.CreateScope();
        var _immediateRequestRepository = scope.ServiceProvider.GetRequiredService<ImmediateRequestRepository>();
        await _immediateRequestRepository.DeleteByCarPlate(request.LicencePlate);
        await _chargeManager.RemoveImmediateRequestByCarPlate(request.LicencePlate);

        if (car.ParkingSlotId != null)
        {
            var parkingSlot = await _parkingSlotRepository.GetByIdAsync(car.ParkingSlotId.Value);
            parkingSlot.Status = ParkingSlotStatus.Free;
            await _parkingSlotRepository.UpdateAsync(parkingSlot);
        }

        car.Status = CarStatus.OutOfParking;
        car.ParkingId = null;
        car.ParkingSlotId = null;
        await _carRepository.UpdateAsync(car);

        var reservations = await _reservationRepository.UpdateCarIsInside(request.LicencePlate, request.ParkingId, false);

        var stopover = await _stopoverRepository.GetFirstByCarPlate(request.LicencePlate);
        if (stopover != null)
        {
            var _parkingRepository = scope.ServiceProvider.GetRequiredService<ParkingRepository>();

            stopover.EndStopoverTime = DateTime.Now;
            var duration = stopover.EndStopoverTime.Value - stopover.StartStopoverTime.Value;
            var totalMinutes = duration.TotalMinutes;

            var parkingSlot = await _parkingSlotRepository.GetByIdAsync(stopover.ParkingSlotId.Value);
            var parking = await _parkingRepository.GetByParkingSlotId(parkingSlot.Id);

            stopover.TotalCost = Math.Round((decimal)totalMinutes * parking.StopCostPerMinute, 2);
            stopover.ToPay = true;
            await _stopoverRepository.UpdateAsync(stopover);
        }
    }

    [HttpPost("park")]
    [Authorize]
    public async Task<IActionResult> Parcheggio([FromBody] Request request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.LicencePlate))
        {
            return BadRequest("Targa non valida");
        }

        var userId = (await _userManager.GetUserAsync(User))?.Id.ToString();
        _logger.LogInformation("L'utente {user} richiede parcheggio per la targa {LicencePlate}", userId, request.LicencePlate);

        var scope = _serviceScopeFactory.CreateScope();
        var _stopoverRepository = scope.ServiceProvider.GetRequiredService<StopoverRepository>();

        var freeSlot = await _parkingSlotRepository.GetFreeParkingSlot(request.ParkingId);
        if (freeSlot is null)
        {
            return Ok("Nessun posto libero disponibile, riprova più tardi");
        }
        freeSlot.Status = ParkingSlotStatus.Occupied;
        await _parkingSlotRepository.UpdateAsync(freeSlot);

        var car = await _carRepository.GetCarByLicencePlate(request.LicencePlate);
        if (car is null)
        {
            return NotFound("Auto non trovata");
        }
        car.ParkingSlotId = freeSlot.Id;
        await _carRepository.UpdateAsync(car);

        var stopover = new Stopover
        {
            StartStopoverTime = DateTime.Now,
            UserId = userId,
            CarPlate = request.LicencePlate,
            ParkingSlotId = freeSlot?.Id,
            TotalCost = 0,
            ToPay = false,
        };
        await _stopoverRepository.AddAsync(stopover);
        stopover.ParkingSlot = freeSlot;

        // TODO: implement update elsewhere
        await _carRepository.UpdateCarStatus(request.LicencePlate, CarStatus.Parked);

        return Ok("Selezionata sosta. Riservato posto numero: " + stopover.ParkingSlot.Number);
    }

    [HttpPost("recharge")]
    [Authorize]
    public async Task<IActionResult> Ricarica([FromBody] Request request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.LicencePlate))
        {
            return BadRequest("Targa non valida");
        }

        var userId = (await _userManager.GetUserAsync(User))?.Id.ToString();
        _logger.LogInformation("L'utente {user} richiede ricarica per la targa {LicencePlate}", userId, request.LicencePlate);

        var reservations = await _reservationRepository.UpdateCarIsInside(request.LicencePlate, request.ParkingId, true);
        await _chargeManager.UpdateReservationsCarIsInside(request.LicencePlate, request.ParkingId, true);
        if (!reservations.Any())
        {
            var scope = _serviceScopeFactory.CreateScope();
            var immediateRequestRepository = scope.ServiceProvider.GetRequiredService<ImmediateRequestRepository>();

            _logger.LogDebug("No reservation found for car {car}, proceding to create ImmediateRequest", request.LicencePlate);

            var existingRequest = await immediateRequestRepository.GetByCarPlate(request.LicencePlate);
            if (existingRequest is not null)
            {
               return BadRequest("Richiesta già in corso");
            }

            var immediateRequest = new ImmediateRequest
            {
                RequestDate = DateTime.Now,
                RequestedChargeLevel = request.ChargeLevel ?? 100,
                CarPlate = request.LicencePlate,
                UserId = userId,
                FromReservation = false,
            };
            await immediateRequestRepository.AddAsync(immediateRequest);
            var chargeIR = await _chargeManager.AddImmediateRequest(immediateRequest, request.ParkingId);

            return Ok(immediateRequest);
        }

        return Ok(reservations);
    }
}

public class Request
{
    public string LicencePlate { get; set; }
    public int ParkingId { get; set; }
    public decimal? ChargeLevel { get; set; }
}
