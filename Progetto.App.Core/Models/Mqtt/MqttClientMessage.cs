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
}

/// <summary>
/// Represents a message sent by the MQTT client
/// </summary>
public class MqttClientMessage : MwBot
{
    public MessageType MessageType { get; set; }
    public int? ParkingSlotId { get; set; }
    public decimal? CurrentCarCharge { get; set; }
    public decimal? TargetBatteryPercentage { get; set; }
    public string? UserId { get; internal set; }
    public string? CarPlate { get; internal set; }
    public CurrentlyCharging? CurrentlyCharging { get; set; }
}
