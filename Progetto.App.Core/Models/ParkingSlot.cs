﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Progetto.App.Core.Models;

public enum ParkingSlotStatus
{
    Free, //green   
    Occupied, //red
    Reserved, //orange
    OutOfService //grey
}

/// <summary>
/// Represents a parking slot (single slot in a parking)
/// </summary>
public class ParkingSlot
{
    public int Id { get; set; }
    public int Number { get; set; }
    public ParkingSlotStatus Status { get; set; }
    public int ParkingId { get; set; }
    public Parking? Parking { get; set; }

    [NotMapped]
    public string StatusName { get { return Status.ToString(); } }
}
