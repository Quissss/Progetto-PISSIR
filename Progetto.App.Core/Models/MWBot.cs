using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models;

public enum MwBotStatus
{
    Offline, // MwBot is offline
    StandBy, // MwBot is waiting for a car / idle
    ChargingCar, // MwBot is charging a car
    Recharging, // MwBot is recharging
    MovingToSlot, // MwBot is moving to parking slot to charge a car
    MovingToDock, // MwBot is moving to charging station to recharge
}

public class MwBot
{
    public int Id { get; set; }
    public decimal BatteryPercentage { get; set; }
    public MwBotStatus Status { get; set; }
}
