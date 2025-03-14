﻿using Progetto.App.Core.Models.Users;

namespace Progetto.App.Core.Models;

public class ImmediateRequest
{
    public int Id { get; set; }
    public DateTime RequestDate { get; set; } // Date when user arrives at the parking
    public decimal RequestedChargeLevel { get; set; } // Requested charge level
    public string? CarPlate { get; set; }
    public Car? Car { get; set; }
    public int ParkingId { get; set; }
    public Parking? Parking { get; set; }
    public int? ParkingSlotId { get; set; }
    public ParkingSlot? ParkingSlot { get; set; }
    public string UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public bool FromReservation { get; set; } // True if the request comes from a reservation
    public bool IsBeingHandled { get; set; } // True if the request is being handled
}
