using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models;

public enum ParkSlotStatus
{
    Free,
    Occupied,
    Reserved,
    OutOfService
}

/// <summary>
/// Represents a parking slot (single slot in a parking)
/// </summary>
public class ParkingSlot
{
    public int Id { get; set; }
    public int Number { get; set; }
    public ParkSlotStatus Status { get; set; }
    public int ParkingId { get; set; }
    public Parking? Parking { get; set; }
}
