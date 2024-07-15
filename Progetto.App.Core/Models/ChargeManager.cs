using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models;

public class ChargeManager
{
    private List<Reservation>? _reservations;
    private Queue<ImmediateRequest>? _immediateRequests;
    private readonly ILogger<ChargeManager> _logger;

    public ChargeManager()
    {
        _reservations = new List<Reservation>();
        _immediateRequests = new Queue<ImmediateRequest>();
        _logger = new Logger<ChargeManager>(new LoggerFactory());
    }

    // Add premium reservation
    public void AddReservation(Reservation reservation)
    {
        _reservations?.Add(reservation);
        _reservations = _reservations?.OrderBy(r => r.ReservationTime).ToList();
    }

    // Add immediate request by base user
    public void AddImmediateRequest(ImmediateRequest immediateRequest)
    {
        _immediateRequests?.Enqueue(immediateRequest);
    }

    // Get next reservation in list
    public Reservation? GetNextReservation()
    {
        if (_reservations?.Count > 0)
        {
            var nextReservation = _reservations[0];
            _reservations.RemoveAt(0);
            return nextReservation;
        }
        return null;
    }

    // Get next immediate request in queue
    public ImmediateRequest? GetNextImmediateRequest()
    {
        if (_immediateRequests?.Count > 0)
        {
            return _immediateRequests.Dequeue();
        }
        return null;
    }

    // Determine which request to serve next
    public object? ServeNext()
    {
        if (_immediateRequests?.Count > 0)
        {
            var immediateRequest = GetNextImmediateRequest();
            _logger.LogInformation($"Serving immediate request from user {immediateRequest?.UserId} at {immediateRequest?.RequestDate}.");
            return immediateRequest;
        }

        if (_reservations?.Count > 0)
        {
            var nextReservation = GetNextReservation();
            _logger.LogInformation($"Serving reservation from user {nextReservation?.UserId} for {nextReservation?.ReservationTime}.");
            return nextReservation;
        }

        _logger.LogInformation("No requests to serve.");
        return null;
    }
}
