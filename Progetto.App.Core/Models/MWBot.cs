using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models;

public enum MwBotStatus
{
    InCharge,
    InUse,
    StandBy,
    Offline // TODO : Add Offline as default status
}

public class MwBot
{
    public int Id { get; set; }
    public decimal BatteryPercentage { get; set; }
    public MwBotStatus Status { get; set; }
    public string? WebToken { get; set; }
}
