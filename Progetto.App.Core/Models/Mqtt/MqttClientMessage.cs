using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models.Mqtt;

public enum MessageType
{
    RequestCharge, // client → broker
    StartCharging, // broker → client
    CompleteCharge, // client → broker
    UpdateCharging, // client → broker
    UpdateMwBot, // client → broker
    UpdateParkingSlot, // client → broker
    StartRecharge, // broker → client
    RequestRecharge, // client → broker
    RequestResumeCharging, // client → broker
    ResumeCharging, // broker → client
    DisconnectClient, // client → broker
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
    public int? ImmediateRequestId { get; set; }
    public ImmediateRequest? ImmediateRequest { get; set; }
}
