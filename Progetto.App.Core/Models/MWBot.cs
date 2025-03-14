﻿namespace Progetto.App.Core.Models;

public enum MwBotStatus
{
    Offline, // MwBot is offline
    StandBy, // MwBot is waiting for a car / idle
    MovingToSlot, // MwBot is moving to parking slot to charge a car
    ChargingCar, // MwBot is charging a car
    MovingToDock, // MwBot is moving to charging station to recharge
    Recharging, // MwBot is recharging
}

public enum MwBotLocations
{
    InDock, // MwBot is in charging station
    InSlot, // MwBot is in parking slot
}

/// <summary>
/// Represents a MwBot (is also used in MqttClientMessage)
/// </summary>
public class MwBot
{
    public int Id { get; set; }
    public decimal BatteryPercentage { get; set; }
    public MwBotStatus Status { get; set; }
    public int? ParkingId { get; set; }
    public Parking? Parking { get; set; }
    public MwBotLocations? LatestLocation { get; set; }
}
