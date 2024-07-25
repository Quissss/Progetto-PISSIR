using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models;

/// <summary>
/// Represents parking cars history (payment historty)
/// </summary>
public class StopoverHistory
{
    public int Id { get; set; }
    public DateTime StartStopoverTime { get; set; }
    public DateTime EndStopoverTime { get; set; }
    public string UserId { get; set; }
    public IdentityUser? User { get; set; }
    public string CarPlate { get; set; }
    public Car? Car { get; set; }
    public decimal TotalCost { get; set; }
}
