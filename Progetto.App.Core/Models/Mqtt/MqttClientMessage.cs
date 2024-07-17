using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models.Mqtt;

/// <summary>
/// Represents a message sent by the MQTT client
/// </summary>
public class MqttClientMessage : MwBot
{
    public int? ParkingSlotId { get; set; }
    public decimal? CurrentCarCharge { get; set; }
    public decimal? TargetBatteryPercentage { get; set; }
    public string? UserId { get; internal set; }
    public string? CarPlate { get; internal set; }
}
