using Microsoft.AspNetCore.Identity;

namespace Progetto.App.Core.Models;

/// <summary>
/// Represents payment history
/// </summary>
public class PaymentHistory
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal? StartChargePercentage { get; set; }
    public decimal? EndChargePercentage { get; set; }
    public string UserId { get; set; }
    public IdentityUser? User { get; set; }
    public string CarPlate { get; set; }
    public Car? Car { get; set; }
    public decimal? EnergyConsumed { get; set; }
    public decimal TotalCost { get; set; }
    public bool IsCharge { get; set; }
}
