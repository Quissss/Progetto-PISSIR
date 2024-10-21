using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Services.SignalR.Hubs;

namespace Progetto.App.Core.Models;

public class ChargeManager : IDisposable
{
    private List<Reservation>? _reservations;
    private Queue<ImmediateRequest>? _immediateRequests;

    private readonly ILogger<ChargeManager> _logger;
    private readonly IHubContext<CarHub> _carHubContext;
    private readonly IHubContext<ParkingSlotHub> _parkingSlotHubContext;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly SemaphoreSlim _reservationsSemaphore = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _immediateRequestsSemaphore = new SemaphoreSlim(1, 1);

    private readonly ImmediateRequestRepository _immediateRequestRepository;
    private readonly ReservationRepository _reservationRepository;

    private Timer? _reservationCheckTimer;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check interval for reservations cleanup
    private readonly TimeSpan _expirationTime = TimeSpan.FromMinutes(2); // Expiration time for reservations

    public ChargeManager(ILogger<ChargeManager> logger, IServiceScopeFactory serviceScopeFactory, IHubContext<CarHub> carHubContext, IHubContext<ParkingSlotHub> parkingSlotHubContext)
    {
        _reservations = new List<Reservation>();
        _immediateRequests = new Queue<ImmediateRequest>();
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;

        _carHubContext = carHubContext;
        _parkingSlotHubContext = parkingSlotHubContext;

        var provider = _serviceScopeFactory.CreateScope().ServiceProvider;
        _immediateRequestRepository = provider.GetRequiredService<ImmediateRequestRepository>();
        _reservationRepository = provider.GetRequiredService<ReservationRepository>();

        GetReservations().GetAwaiter().GetResult();
        GetImmediateRequests().GetAwaiter().GetResult();

        _reservationCheckTimer = new Timer(CheckExpiredReservations, null, TimeSpan.Zero, _checkInterval);
    }

    private async void CheckExpiredReservations(object? state)
    {
        _logger.LogDebug("Checking for expired reservations...");

        await _reservationsSemaphore.WaitAsync();
        try
        {
            // Filter outdated reservations
            var expiredReservations = _reservations?.Where(r => r.RequestDate.HasValue && (DateTime.Now - r.RequestDate.Value) > _expirationTime).ToList();

            if (expiredReservations?.Count > 0)
            {
                foreach (var reservation in expiredReservations)
                {
                    _logger.LogInformation("Removing expired reservation with ID {Id}", reservation.Id);

                    _reservations?.Remove(reservation);

                    var provider = _serviceScopeFactory.CreateScope().ServiceProvider;
                    var reservationRepository = provider.GetRequiredService<ReservationRepository>();
                    await reservationRepository.DeleteAsync(r => r.Id == reservation.Id);
                }
            }
            else
            {
                _logger.LogDebug("No expired reservations found.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while checking for expired reservations.");
        }
        finally
        {
            _reservationsSemaphore.Release();
        }
    }

    public async Task RemoveImmediateRequestByCarPlate(string carPlate)
    {
        await _immediateRequestsSemaphore.WaitAsync();
        try
        {
            var immediateRequestsList = _immediateRequests?.ToList();

            if (immediateRequestsList?.Count == 0)
            {
                _logger.LogInformation("No immediate requests");
                return;
            }

            var immediateRequestToRemove = immediateRequestsList.FirstOrDefault(ir => ir.CarPlate == carPlate);

            if (immediateRequestToRemove != null)
            {
                immediateRequestsList.Remove(immediateRequestToRemove);
                _immediateRequests = new Queue<ImmediateRequest>(immediateRequestsList);
                _logger.LogInformation("Removed immediate request with ID {id}", carPlate);
            }
            else
            {
                _logger.LogWarning("Immediate request with ID {id} not found", carPlate);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while removing immediate request with ID {id}", carPlate);
        }
        finally
        {
            _immediateRequestsSemaphore.Release();
        }
    }

    public async Task UpdateReservationsCarIsInside(string plate, int parkingId, bool carInside)
    {
        _reservations?.ForEach(r =>
        {
            if (r.CarPlate == plate && r.ParkingId == parkingId)
            {
                r.CarIsInside = carInside;
            }
        });
    }

    private async Task GetReservations()
    {
        _logger.BeginScope("Retrieving reservations");
        try
        {
            var reservations = await _reservationRepository.GetAllAsync();
            _reservations = reservations.OrderBy(r => r.ReservationTime).ToList();
            _logger.LogInformation("Retrieved {count} reservations", _reservations.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while retrieving reservations");
        }
    }

    private async Task GetImmediateRequests()
    {
        _logger.BeginScope("Retrieving immediate requests");
        try
        {
            var immediateRequests = await _immediateRequestRepository.GetAllWithNoReservation();
            if (immediateRequests?.Count() > 0)
                _immediateRequests = new Queue<ImmediateRequest>(immediateRequests.OrderBy(ir => ir?.RequestDate).Cast<ImmediateRequest>());
            _logger.LogInformation("Retrieved {count} immediate requests", _immediateRequests?.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while retrieving immediate requests");
        }
    }

    public void AddReservation(Reservation reservation)
    {
        _reservations?.Add(reservation);
    }

    public async Task<ImmediateRequest> AddImmediateRequest(ImmediateRequest immediateRequest, int parkingId)
    {
        while (true)
        {
            var provider = _serviceScopeFactory.CreateScope().ServiceProvider;
            var _parkingSlotRepository = provider.GetRequiredService<ParkingSlotRepository>();

            var allFreeSlots = await _parkingSlotRepository.GetFreeParkingSlots(parkingId);
            if (allFreeSlots.Any())
            {
                var freeSlot = allFreeSlots.FirstOrDefault();
                if (freeSlot != null)
                {
                    immediateRequest.ParkingSlotId = freeSlot.Id;

                    _immediateRequests?.Enqueue(immediateRequest);
                    await _immediateRequestRepository.UpdateAsync(immediateRequest);

                    freeSlot.Status = ParkingSlotStatus.Occupied;
                    await _parkingSlotRepository.UpdateAsync(freeSlot);
                    await _parkingSlotHubContext.Clients.All.SendAsync("ParkingSlotUpdated", freeSlot);

                    return immediateRequest;
                }
            }

            Task.Delay(5000).Wait();
        }
    }

    private Reservation? GetNextReservation()
    {
        if (_reservations?.Count > 0)
        {
            var nextReservation = _reservations.FirstOrDefault(r => r.ReservationTime <= DateTime.Now);
            if (nextReservation is not null)
            {
                _reservations.Remove(nextReservation);
                return nextReservation;
            }
            return nextReservation;
        }
        return null;
    }

    private ImmediateRequest? GetNextImmediateRequest()
    {
        if (_immediateRequests?.Count > 0)
        {
            return _immediateRequests.Dequeue();
        }
        return null;
    }

    public async Task<ImmediateRequest?> ServeNext(MwBot mwBot)
    {
        ImmediateRequest? immediateRequest = null;

        // Car entered the parking, serve reservations
        try
        {
            await _reservationsSemaphore.WaitAsync();

            var parkingReservations = _reservations?.Where(r => r.ParkingId == mwBot.ParkingId && r.RequestDate <= DateTime.Now && r.CarIsInside);
            if (parkingReservations?.Count() > 0)
            {
                var provider = _serviceScopeFactory.CreateScope().ServiceProvider;
                var _carRepository = provider.GetRequiredService<CarRepository>();
                var _parkingRepository = provider.GetRequiredService<ParkingRepository>();
                var _parkingSlotRepository = provider.GetRequiredService<ParkingSlotRepository>();

                _logger.LogInformation("MwBot {mwBot}: Found reservations, checking free slots", mwBot.Id);

                var freeSlot = await _parkingSlotRepository.GetFreeParkingSlot(mwBot.ParkingId);
                if (freeSlot is null) return null;

                var nextReservation = GetNextReservation();
                if (nextReservation is null) return null;

                _logger.LogInformation("MwBot {mwBot}: Generating immediate request", mwBot.Id);

                immediateRequest = await _immediateRequestRepository.AddAsync(
                    new ImmediateRequest
                    {
                        RequestDate = DateTime.Now,
                        CarPlate = nextReservation.CarPlate,
                        RequestedChargeLevel = nextReservation.RequestedChargeLevel,
                        ParkingId = mwBot.ParkingId.Value,
                        Parking = await _parkingRepository.GetByIdAsync(mwBot.ParkingId.Value),
                        ParkingSlotId = freeSlot.Id,
                        ParkingSlot = await _parkingSlotRepository.GetByIdAsync(freeSlot.Id),
                        UserId = nextReservation.UserId,
                        FromReservation = true
                    }
                );
                if (immediateRequest is null) return null;

                await _reservationRepository.DeleteAsync(r => r.Id == nextReservation.Id);
                _logger.LogInformation("MwBot {mwBot}: Serving reservation from user {nextReservation?.UserId} for reservation time {nextReservation?.ReservationTime}.", mwBot.Id, nextReservation?.UserId, nextReservation?.ReservationTime);

                freeSlot.Status = ParkingSlotStatus.Occupied;
                await _parkingSlotRepository.UpdateAsync(freeSlot);
                await _parkingSlotHubContext.Clients.All.SendAsync("ParkingSlotUpdated", freeSlot);

                if (nextReservation?.CarPlate is not null)
                {
                    var car = await _carRepository.GetCarByLicencePlate(nextReservation.CarPlate);
                    if (car is null) return null;
                    car.Status = CarStatus.InCharge;
                    await _carRepository.UpdateAsync(car);
                    await _carHubContext.Clients.All.SendAsync("CarUpdated", car);
                }

                _logger.LogInformation("MwBot {mwBot}: Created immediate request for user {immediateRequest.UserId} at {immediateRequest.RequestDate}.", mwBot.Id, immediateRequest.UserId, immediateRequest.RequestDate);
            }
        }
        finally
        {
            _reservationsSemaphore.Release();
        }

        // Car entered the parking, serve immediate requests
        try
        {
            await _immediateRequestsSemaphore.WaitAsync();

            var scope = _serviceScopeFactory.CreateScope();
            var parkingSlotRepository = scope.ServiceProvider.GetRequiredService<ParkingSlotRepository>();

            var immediateRequests = _immediateRequests?.Where(ir => ir.ParkingId == mwBot.ParkingId && ir.RequestDate <= DateTime.Now);

            if (immediateRequests?.Count() > 0)
            {
                immediateRequest = GetNextImmediateRequest();
                _logger.LogInformation("MwBot {mwBot}: Serving immediate request from user {immediateRequest?.UserId} at {immediateRequest?.RequestDate}.", mwBot.Id, immediateRequest?.UserId, immediateRequest?.RequestDate);
            }
        }
        finally
        {
            _immediateRequestsSemaphore.Release();
        }

        if (immediateRequest is null)
        {
            _logger.LogDebug("MwBot {mwBot}: No requests to serve.", mwBot.Id);
        }

        return immediateRequest;
    }

    public void Dispose()
    {
        _reservationCheckTimer?.Dispose();
    }
}
