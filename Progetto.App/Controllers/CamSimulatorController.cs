using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Progetto.App.Core.Models;
using Progetto.App.Core.Models.Users;
using Progetto.App.Core.Repositories;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Progetto.App.Core.Services.SignalR.Hubs;

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
    private readonly IHubContext<CarHub> _carHubContext;
    private readonly IHubContext<ParkingSlotHub> _parkingSlotHubContext;
    private readonly ILogger<CamSimulatorController> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly UserManager<ApplicationUser> _userManager;

    public CamSimulatorController(
        CarRepository carRepository,
        IHubContext<CarHub> hubContext,
        IHubContext<ParkingSlotHub> parkingSlotHubContext,
        ILogger<CamSimulatorController> logger,
        IServiceScopeFactory serviceScopeFactory,
        ReservationRepository reservationRepository,
        ChargeManager chargeManager,
        ParkingSlotRepository parkingSlotRepository,
        StopoverRepository stopoverRepository,
        UserManager<ApplicationUser> userManager)
    {
        _carRepository = carRepository;
        _reservationRepository = reservationRepository;
        _logger = logger;
        _carHubContext = hubContext;
        _parkingSlotHubContext = parkingSlotHubContext;
        _serviceScopeFactory = serviceScopeFactory;
        _chargeManager = chargeManager;
        _parkingSlotRepository = parkingSlotRepository;
        _userManager = userManager;
        _stopoverRepository = stopoverRepository;
    }

    [HttpPost("detect")]
    public async Task<IActionResult> Detect([FromBody] Request request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.CarPlate))
        {
            return BadRequest("Targa non valida");
        }

        // Simulazione dell'elaborazione della targa
        string responseMessage = $"Targa rilevata: {request.CarPlate} sul parcheggio n.{request.ParkingId}";
        var car = await _carRepository.GetCarByPlate(request.CarPlate);

        if (car == null)
        {
            _logger.LogInformation("Carplate {plate} not found", request.CarPlate);
            return NotFound(responseMessage);
        }

        if (car.Status == CarStatus.OutOfParking)
        {
            responseMessage = await CarEntering(request, car);
        }
        else
        {
            responseMessage = await CarDeparting(request, car);
        }

        return Ok(responseMessage);
    }

    private async Task<string> CarEntering(Request request, Car car)
    {
        var responseMessage = $"Detected carplate {request.CarPlate} on arrival";
        _logger.LogInformation(responseMessage);

        car.Status = CarStatus.Waiting;
        car.ParkingId = request.ParkingId;
        await _carRepository.UpdateAsync(car);
        await _carHubContext.Clients.All.SendAsync("CarUpdated", car);

        var reservations = await _reservationRepository.UpdateCarIsInside(request.CarPlate, request.ParkingId, true);
        if (reservations.Any())
        {
            _logger.LogDebug($"Reservations found for car {request.CarPlate}, setting car is inside to true");
            await _chargeManager.UpdateReservationsCarIsInside(request.CarPlate, request.ParkingId, true);

            car.Status = CarStatus.WaitForCharge;
            await _carRepository.UpdateAsync(car);
            await _carHubContext.Clients.All.SendAsync("CarUpdated", car);
        }

        return responseMessage;
    }

    private async Task<string> CarDeparting(Request request, Car car)
    {
        var responseMessage = $"Detected carplate {request.CarPlate} on departure";

        if (car.ParkingId != request.ParkingId)
        {
            responseMessage = $"Carplate {request.CarPlate} is not in parking {request.ParkingId}";
            _logger.LogWarning(responseMessage);
            return responseMessage;
        }

        _logger.LogInformation("Detected carplate {plate} on departure", request.CarPlate);

        // Remove related requests
        var scope = _serviceScopeFactory.CreateScope();
        var _immediateRequestRepository = scope.ServiceProvider.GetRequiredService<ImmediateRequestRepository>();
        await _immediateRequestRepository.DeleteByCarPlate(request.CarPlate);
        await _chargeManager.RemoveImmediateRequestByCarPlate(request.CarPlate);

        if (car.ParkingSlotId != null)
        {
            var parkingSlot = await _parkingSlotRepository.GetByIdAsync(car.ParkingSlotId.Value);
            parkingSlot.Status = ParkingSlotStatus.Free;
            await _parkingSlotRepository.UpdateAsync(parkingSlot);
            await _parkingSlotHubContext.Clients.All.SendAsync("ParkingSlotUpdated", parkingSlot);
        }

        car.Status = CarStatus.OutOfParking;
        car.ParkingId = null;
        car.ParkingSlotId = null;
        await _carRepository.UpdateAsync(car);
        await _carHubContext.Clients.All.SendAsync("CarUpdated", car);

        var reservations = await _reservationRepository.UpdateCarIsInside(request.CarPlate, request.ParkingId, false);

        var stopover = await _stopoverRepository.GetFirstByCarPlate(request.CarPlate);
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

        return responseMessage;
    }

    [HttpPost("park")]
    [Authorize]
    public async Task<IActionResult> Parcheggio([FromBody] Request request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.CarPlate))
        {
            return BadRequest("Targa non valida");
        }

        var userId = (await _userManager.GetUserAsync(User))?.Id.ToString();
        _logger.LogInformation("L'utente {user} richiede parcheggio per la targa {Plate}", userId, request.CarPlate);

        var scope = _serviceScopeFactory.CreateScope();
        var _stopoverRepository = scope.ServiceProvider.GetRequiredService<StopoverRepository>();

        var freeSlot = await _parkingSlotRepository.GetFreeParkingSlot(request.ParkingId);
        if (freeSlot is null)
        {
            return Ok("Nessun posto libero disponibile, riprova più tardi");
        }
        freeSlot.Status = ParkingSlotStatus.Occupied;
        await _parkingSlotRepository.UpdateAsync(freeSlot);
        await _parkingSlotHubContext.Clients.All.SendAsync("ParkingSlotUpdated", freeSlot);

        var car = await _carRepository.GetCarByPlate(request.CarPlate);
        if (car is null)
        {
            return NotFound("Auto non trovata");
        }
        car.ParkingSlotId = freeSlot.Id;
        car.Status = CarStatus.WaitForParking;
        await _carRepository.UpdateAsync(car);
        await _carHubContext.Clients.All.SendAsync("CarUpdated", car);

        var stopover = new Stopover
        {
            StartStopoverTime = DateTime.Now,
            UserId = userId,
            CarPlate = request.CarPlate,
            ParkingSlotId = freeSlot?.Id,
            TotalCost = 0,
            ToPay = false,
        };
        await _stopoverRepository.AddAsync(stopover);
        stopover.ParkingSlot = freeSlot;

        await _carRepository.UpdateCarStatus(request.CarPlate, CarStatus.Parked);
        await _carHubContext.Clients.All.SendAsync("CarUpdated", car);

        return Ok("Selezionata sosta. Riservato posto numero: " + stopover.ParkingSlot.Number);
    }

    [HttpPost("recharge")]
    [Authorize]
    public async Task<IActionResult> Ricarica([FromBody] Request request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.CarPlate))
        {
            return BadRequest("Targa non valida");
        }

        var userId = (await _userManager.GetUserAsync(User))?.Id.ToString();
        _logger.LogInformation("User {user} requests recharge for car {plate}", userId, request.CarPlate);

        var reservations = await _reservationRepository.UpdateCarIsInside(request.CarPlate, request.ParkingId, true);
        await _chargeManager.UpdateReservationsCarIsInside(request.CarPlate, request.ParkingId, true);

        if (!reservations.Any())
        {
            var scope = _serviceScopeFactory.CreateScope();
            var parkingRepository = scope.ServiceProvider.GetRequiredService<ParkingRepository>();
            var immediateRequestRepository = scope.ServiceProvider.GetRequiredService<ImmediateRequestRepository>();

            _logger.LogDebug("No reservation found for car {car}, proceding to create ImmediateRequest", request.CarPlate);

            var existingRequest = await immediateRequestRepository.GetByCarPlate(request.CarPlate);
            if (existingRequest is not null)
            {
                return BadRequest("Richiesta già in corso");
            }

            var car = await _carRepository.UpdateCarStatus(request.CarPlate, CarStatus.WaitForCharge);
            await _carHubContext.Clients.All.SendAsync("CarUpdated", car);

            var immediateRequest = new ImmediateRequest
            {
                RequestDate = DateTime.Now,
                RequestedChargeLevel = request.ChargeLevel ?? 100,
                ParkingId = request.ParkingId,
                Parking = await parkingRepository.GetByIdAsync(request.ParkingId),
                CarPlate = request.CarPlate,
                UserId = userId,
                FromReservation = false,
            };
            await immediateRequestRepository.AddAsync(immediateRequest);
            var chargeIR = await _chargeManager.AddImmediateRequest(immediateRequest, request.ParkingId);

            var personInQueue = reservations.Count(r => r.RequestDate < immediateRequest.RequestDate);
            return Ok(personInQueue + 1);
        }

        return Ok(reservations);
    }
}

public class Request
{
    public string CarPlate { get; set; }
    public int ParkingId { get; set; }
    public decimal? ChargeLevel { get; set; }
}
