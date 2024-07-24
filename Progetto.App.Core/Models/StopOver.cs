using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models;

/// <summary>
/// Represents parking cars (for stopover billing)
/// </summary>
public class StopOver
{
    public int Id { get; set; }
    public DateTime? StartStopOverTime { get; set; } // set on car arrival (record generation)
    public DateTime? EndStopoverTime { get; set; } // set on car departure (cam detects a car currently in StopOver/CurrentlyCharging it means it's departing)
    public string? UserId { get; set; } // set on car arrival
    public IdentityUser? User { get; set; }
    public string? CarPlate { get; set; } // set on car arrival
    public Car? Car { get; set; }
    public int? ParkingId { get; set; } // set on car arrival
    public Parking? Parking { get; set; }
    public decimal TotalCost { get; set; } // set on car departure
    public bool ToPay { get; set; } // set on car departure
}
