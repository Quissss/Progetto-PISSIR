using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Progetto.App.Core.Models;

public enum CarStatus
{
    OutOfParking,   //0
    Waiting,        //1
    WaitForCharge,  //2
    WaitForParking, //3
    InCharge,       //4
    Charged,        //5
    Parked          //6
}

/// <summary>
/// Represents a car
/// </summary>
public class Car
{
    public string LicencePlate { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public bool IsElectric { get; set; }
    public CarStatus Status { get; set; }
    public int? ParkingId { get; set; }
    public Parking? Parking { get; set; }
    public int? ParkingSlotId { get; set; }
    public string OwnerId { get; set; }
    public IdentityUser? Owner { get; set; }

    [NotMapped]
    public string StatusName { get { return Status.ToString(); } }
}
