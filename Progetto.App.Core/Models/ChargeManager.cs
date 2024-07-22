using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Progetto.App.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models
{
    public class ChargeManager
    {
        private List<Reservation>? _reservations;
        private Queue<ImmediateRequest>? _immediateRequests;
        private readonly ILogger<ChargeManager> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly SemaphoreSlim _reservationsSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _immediateRequestsSemaphore = new SemaphoreSlim(1, 1);

        public ChargeManager(ILogger<ChargeManager> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _reservations = new List<Reservation>();
            _immediateRequests = new Queue<ImmediateRequest>();
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;

            GetReservations().GetAwaiter().GetResult();
            GetImmediateRequests().GetAwaiter().GetResult();
        }

        private async Task GetReservations()
        {
            _logger.BeginScope("Retrieving reservations");
            try
            {
                var scope = _serviceScopeFactory.CreateScope();
                var _reservationRepository = scope.ServiceProvider.GetRequiredService<ReservationRepository>();

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
                var scope = _serviceScopeFactory.CreateScope();
                var _immediateRequestRepository = scope.ServiceProvider.GetRequiredService<ImmediateRequestRepository>();

                var immediateRequests = await _immediateRequestRepository.GetAllAsync();
                _immediateRequests = new Queue<ImmediateRequest>(immediateRequests.OrderBy(ir => ir.RequestDate));
                _logger.LogInformation("Retrieved {count} immediate requests", _immediateRequests.Count);
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

        public void AddImmediateRequest(ImmediateRequest immediateRequest)
        {
            _immediateRequests?.Enqueue(immediateRequest);
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

            // Serve reservations
            try
            {
                await _reservationsSemaphore.WaitAsync();
                var parkingReservations = _reservations?.Where(r => r.ParkingId == mwBot.ParkingId && r.RequestDate <= DateTime.Now);
                if (parkingReservations?.Count() > 0)
                {
                    var scope = _serviceScopeFactory.CreateScope();
                    var _parkingSlotRepository = scope.ServiceProvider.GetRequiredService<ParkingSlotRepository>();
                    var _immediateRequestRepository = scope.ServiceProvider.GetRequiredService<ImmediateRequestRepository>();
                    var _reservationsRepository = scope.ServiceProvider.GetRequiredService<ReservationRepository>();

                    _logger.LogInformation("MwBot {mwBot}: Found reservations, checking free slots", mwBot.Id);
                    var allFreeSlots = await _parkingSlotRepository.GetFreeParkingSlots(mwBot.ParkingId);
                    if (allFreeSlots is null || allFreeSlots.Count() == 0) return null;

                    var freeSlot = allFreeSlots.FirstOrDefault();
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
                            ParkingSlotId = freeSlot.Id,
                            ParkingSlot = _parkingSlotRepository.GetByIdAsync(freeSlot.Id).GetAwaiter().GetResult(),
                            UserId = nextReservation.UserId,
                            FromReservation = true
                        }
                    );
                    if (immediateRequest is null) return null;

                    //await _reservationsRepository.DeleteAsync(r => r.Id == nextReservation.Id);
                    _logger.LogInformation("MwBot {mwBot}: Serving reservation from user {nextReservation?.UserId} for reservation time {nextReservation?.ReservationTime}.", mwBot.Id, nextReservation?.UserId, nextReservation?.ReservationTime);

                    freeSlot.Status = ParkingSlotStatus.Occupied;
                    await _parkingSlotRepository.UpdateAsync(freeSlot);

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
                if (_immediateRequests?.Count > 0)
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
                _logger.LogInformation("MwBot {mwBot}: No requests to serve.", mwBot.Id);
            }

            return immediateRequest;
        }
    }
}
