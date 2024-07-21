using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models.Mqtt;

public enum MessageType
{
    RequestCharge, // client → server
    StartCharging, // server → client
    CompleteCharge, // client → server
    UpdateCharging, // client → server
    UpdateMwBot, // client → server
    UpdateParkingSlot, // client → server
    StartRecharge, // server → client
    RequestRecharge, // client → server
}

/// <summary>
/// Represents a message sent by the MQTT client
/// </summary>
public class MqttClientMessage : MwBot
{
    public MessageType MessageType { get; set; }
    public int? ParkingSlotId { get; set; }
    public ParkingSlot? ParkingSlot { get; set; }
    public decimal? CurrentCarCharge { get; set; }
    public decimal? TargetBatteryPercentage { get; set; }
    public string? UserId { get; set; }
    public string? CarPlate { get; set; }
    public CurrentlyCharging? CurrentlyCharging { get; set; }
}
