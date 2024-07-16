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
    public DateTime StartDate { get; set; }
    public int MwBotId { get; set; }
    public MwBot? MwBot { get; set; }
    public string UserId { get; set; }
    public IdentityUser? User { get; set; }
    public string CarPlate { get; set; }
    public Car? Car { get; set; }
}
