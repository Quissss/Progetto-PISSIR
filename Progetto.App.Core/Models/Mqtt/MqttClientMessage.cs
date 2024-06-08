using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models.Mqtt;

public class MqttClientMessage : MwBot
{
    public Reservation? Reservation { get; set; }
}
