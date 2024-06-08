using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models;

public enum MwBotStatus
{
    InCharge,
    ChargingCar,
    StandBy,
    Offline
}

public class MwBot
{
    public int Id { get; set; }
    public decimal BatteryPercentage { get; set; }
    public MwBotStatus Status { get; set; }
}
