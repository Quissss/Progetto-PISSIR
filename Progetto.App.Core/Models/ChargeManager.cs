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
        private ReservationRepository _reservationRepository;
        private ImmediateRequestRepository _immediateRequestRepository;
        private readonly SemaphoreSlim _reservationsSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _immediateRequestsSemaphore = new SemaphoreSlim(1, 1);

        public ChargeManager(ILogger<ChargeManager> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _reservations = new List<Reservation>();
            _immediateRequests = new Queue<ImmediateRequest>();
            _logger = logger;

            var provider = serviceScopeFactory.CreateScope().ServiceProvider;
            _reservationRepository = provider.GetRequiredService<ReservationRepository>();
            _immediateRequestRepository = provider.GetRequiredService<ImmediateRequestRepository>();

            GetReservations().GetAwaiter().GetResult();
            GetImmediateRequests().GetAwaiter().GetResult();
        }

        private async Task GetReservations()
        {
            _logger.BeginScope("Retrieving reservations");
            try
            {
                var reservations = await _reservationRepository.GetAllAsync();
                await _reservationsSemaphore.WaitAsync();
                _reservations = reservations.OrderBy(r => r.ReservationTime).ToList();
                _logger.LogInformation("Retrieved {count} reservations", _reservations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving reservations");
            }
            finally
            {
                _reservationsSemaphore.Release();
            }
        }

        private async Task GetImmediateRequests()
        {
            _logger.BeginScope("Retrieving immediate requests");
            try
            {
                var immediateRequests = await _immediateRequestRepository.GetAllAsync();
                await _immediateRequestsSemaphore.WaitAsync();
                _immediateRequests = new Queue<ImmediateRequest>(immediateRequests.OrderBy(ir => ir.RequestDate));
                _logger.LogInformation("Retrieved {count} immediate requests", _immediateRequests.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving immediate requests");
            }
            finally
            {
                _immediateRequestsSemaphore.Release();
            }
        }

        public async Task AddReservation(Reservation reservation)
        {
            await _reservationsSemaphore.WaitAsync();
            try
            {
                _reservations?.Add(reservation);
                _reservations = _reservations?.OrderBy(r => r.ReservationTime).ToList();
            }
            finally
            {
                _reservationsSemaphore.Release();
            }
        }

        public async Task AddImmediateRequest(ImmediateRequest immediateRequest)
        {
            await _immediateRequestsSemaphore.WaitAsync();
            try
            {
                _immediateRequests?.Enqueue(immediateRequest);
            }
            finally
            {
                _immediateRequestsSemaphore.Release();
            }
        }

        private Reservation? GetNextReservation()
        {
            if (_reservations?.Count > 0)
            {
                var nextReservation = _reservations[0];
                _reservations.RemoveAt(0);
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

        public async Task<ImmediateRequest?> ServeNext(int mwBotId)
        {
            await _reservationsSemaphore.WaitAsync();
            try
            {
                if (_reservations?.Count > 0)
                {
                    var nextReservation = GetNextReservation();
                    if (nextReservation is null)
                        return null;

                    _logger.LogInformation("MwBot {mwBot}: Serving reservation from user {nextReservation?.UserId} for {nextReservation?.ReservationTime}.", mwBotId, nextReservation?.UserId, nextReservation?.ReservationTime);

                    var immediateRequest = await _immediateRequestRepository.AddAsync(
                        new ImmediateRequest
                        {
                            RequestDate = DateTime.Now,
                            RequestedChargeLevel = nextReservation.RequestedChargeLevel,
                            ParkingSlotId = nextReservation.ParkingSlotId,
                            UserId = nextReservation.UserId,
                            FromReservation = true
                        }
                    );
                    if (immediateRequest is null)
                        return null;

                    _logger.LogInformation("MwBot {mwBot}: Created immediate request for user {immediateRequest.UserId} at {immediateRequest.RequestDate}.", mwBotId, immediateRequest.UserId, immediateRequest.RequestDate);

                    return immediateRequest;
                }
            }
            finally
            {
                _reservationsSemaphore.Release();
            }

            await _immediateRequestsSemaphore.WaitAsync();
            try
            {
                if (_immediateRequests?.Count > 0)
                {
                    var immediateRequest = GetNextImmediateRequest();
                    _logger.LogInformation("MwBot {mwBot}: Serving immediate request from user {immediateRequest?.UserId} at {immediateRequest?.RequestDate}.", mwBotId, immediateRequest?.UserId, immediateRequest?.RequestDate);
                    return immediateRequest;
                }
            }
            finally
            {
                _immediateRequestsSemaphore.Release();
            }

            _logger.LogInformation("MwBot {mwBot}: No requests to serve.", mwBotId);
            return null;
        }
    }
}
