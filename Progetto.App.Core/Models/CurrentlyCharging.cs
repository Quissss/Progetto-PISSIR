using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models;

/// <summary>
/// Represents active charging
/// </summary>
public class CurrentlyCharging
{
    public int Id { get; set; }
    public DateTime? StartChargingTime { get; set; }
    public DateTime? EndChargingTime { get; set; }
    public decimal? StartChargePercentage { get; set; }
    public decimal? CurrentChargePercentage { get; set; }
    public decimal? TargetChargePercentage { get; set; }
    public int MwBotId { get; set; }
    public MwBot? MwBot { get; set; }
    public string? UserId { get; set; }
    public IdentityUser? User { get; set; }
    public string? CarPlate { get; set; }
    public Car? Car { get; set; }
    public int? ParkingSlotId { get; set; }
    public ParkingSlot? ParkingSlot { get; set; }
    public decimal EnergyConsumed { get; set; }
    public decimal TotalCost { get; set; }
    public bool ToPay { get; set; }
    public int? ImmediateRequestId { get; set; }
    public ImmediateRequest? ImmediateRequest { get; set; }
}
