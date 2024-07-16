using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models;

public class ImmediateRequest
{
    public int Id { get; set; }
    public DateTime RequestDate { get; set; } // Date when user arrives at the parking
    public decimal RequestedChargeLevel { get; set; } // Requested charge level
    public int ParkingSlotId { get; set; }
    public ParkingSlot ParkingSlot { get; set; }
    public string UserId { get; set; }
    public IdentityUser User { get; set; }
    public bool FromReservation { get; set; } // True if the request comes from a reservation
}
