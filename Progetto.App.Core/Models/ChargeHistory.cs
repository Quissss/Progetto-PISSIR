using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models;

public class ChargeHistory
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal StartChargeLevel { get; set; }
    public decimal EndChargeLevel { get; set; }
    public int MWBotId { get; set; }
    public MWBot MWBot { get; set; }
    public string UserId { get; set; }
    public IdentityUser User { get; set; }
}
