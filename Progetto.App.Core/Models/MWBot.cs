using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models;

public enum MWBotStatus
{
    InCharge,
    InUse,
    StandBy
}

public class MWBot
{
    public int Id { get; set; }
    public decimal BatteryPercentage { get; set; }
    public MWBotStatus Status { get; set; }
}
