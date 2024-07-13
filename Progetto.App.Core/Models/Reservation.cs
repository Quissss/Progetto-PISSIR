using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public int ParkingSlotId { get; set; }
    public ParkingSlot? ParkingSlot { get; set; }
    public string UserId { get; set; }
    public IdentityUser? User { get; set; }
}
