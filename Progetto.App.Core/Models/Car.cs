using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models;

public enum CarStatus
{
    OutOfParking,
    InCharge,
    Waiting,
    Charged
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
    public string OwnerId { get; set; }
    public IdentityUser Owner { get; set; }
}
