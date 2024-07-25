﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models;

public enum CarStatus
{
    OutOfParking,    //0
    InCharge,        //1
    Waiting,         //2
    Charged,         //3
    Parked           //4
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
}
