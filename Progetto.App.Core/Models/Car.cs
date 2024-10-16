using Microsoft.AspNetCore.Identity;
using Progetto.App.Core.Models.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace Progetto.App.Core.Models;

public enum CarStatus
{
    OutOfParking,
    Waiting,
    WaitForCharge,
    WaitForParking,
    InCharge,
    Charged,
    Parked
}

/// <summary>
/// Represents a car
/// </summary>
public class Car
{
    public string Plate { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public bool IsElectric { get; set; }
    public CarStatus Status { get; set; }
    public int? ParkingId { get; set; }
    public Parking? Parking { get; set; }
    public int? ParkingSlotId { get; set; }
    public string OwnerId { get; set; }
    public ApplicationUser? Owner { get; set; }

    [NotMapped]
    public string StatusName { get { return Status.ToString(); } }
}
