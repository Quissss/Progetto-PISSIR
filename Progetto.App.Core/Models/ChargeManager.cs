using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models;

public class ChargeManager
{
    private List<Reservation>? reservations;
    private Queue<ImmediateRequest>? immediateRequests;

    public ChargeManager()
    {
        reservations = new List<Reservation>();
        immediateRequests = new Queue<ImmediateRequest>();
    }

    // Add premium reservation
    public void AddReservation(Reservation reservation)
    {
        reservations?.Add(reservation);
        reservations = reservations?.OrderBy(r => r.ReservationTime).ToList();
    }

    // Add immediate request by base user
    public void AddImmediateRequest(ImmediateRequest immediateRequest)
    {
        immediateRequests?.Enqueue(immediateRequest);
    }

    // Get next reservation in list
    public Reservation GetNextReservation()
    {
        if (reservations?.Count > 0)
        {
            var nextReservation = reservations[0];
            reservations.RemoveAt(0);
            return nextReservation;
        }
        return null;
    }

    // Get next immediate request in queue
    public ImmediateRequest GetNextImmediateRequest()
    {
        if (immediateRequests?.Count > 0)
        {
            return immediateRequests.Dequeue();
        }
        return null;
    }

    // Determine which request to serve next
    public string ServeNext()
    {
        if (immediateRequests?.Count > 0)
        {
            var immediateRequest = GetNextImmediateRequest();
            return $"Serving immediate request from user {immediateRequest.UserId} at {immediateRequest.RequestDate}.";
        }

        if (reservations?.Count > 0)
        {
            var nextReservation = GetNextReservation();
            return $"Serving reservation from user {nextReservation.UserId} for {nextReservation.ReservationTime}.";
        }

        return "No requests to serve.";
    }
}
