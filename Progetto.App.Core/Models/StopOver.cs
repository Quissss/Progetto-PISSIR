using Microsoft.AspNetCore.Identity;
using Progetto.App.Core.Models.Users;

namespace Progetto.App.Core.Models;

/// <summary>
/// Represents parking cars (for stopover billing)
/// </summary>
public class Stopover
{
    public int Id { get; set; }
    public DateTime? StartStopoverTime { get; set; }
    public DateTime? EndStopoverTime { get; set; }
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public string? CarPlate { get; set; }
    public Car? Car { get; set; }
    public int? ParkingSlotId { get; set; }
    public ParkingSlot? ParkingSlot { get; set; }
    public decimal TotalCost { get; set; }
    public bool ToPay { get; set; }
}
