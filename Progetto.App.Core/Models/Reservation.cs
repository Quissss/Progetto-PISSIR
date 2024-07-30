using Microsoft.AspNetCore.Identity;

namespace Progetto.App.Core.Models;

/// <summary>
/// Represents a reservation (only for premium users)
/// </summary>
public class Reservation
{
    public int Id { get; set; }
    public DateTime? ReservationTime { get; set; } // Time when the reservation was made
    public DateTime? RequestDate { get; set; } // Date when the user wants to charge the vehicle
    public decimal RequestedChargeLevel { get; set; } // Requested charge level
    public string? CarPlate { get; set; }
    public Car? Car { get; set; }
    public int ParkingId { get; set; }
    public Parking? Parking { get; set; }
    public string UserId { get; set; }
    public IdentityUser? User { get; set; }
    public bool CarIsInside { get; set; }
}
