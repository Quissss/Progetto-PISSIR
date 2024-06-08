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
    public DateTime ParkStartDate { get; set; }
    public DateTime? ParkEndDate { get; set; }
    public DateTime ChargeStartDate { get; set; }
    public DateTime? ChargeEndDate { get; set; }
    public decimal StartChargeLevel { get; set; }
    public decimal? EndChargeLevel { get; set; }
    public int? MWBotId { get; set; }
    public MwBot? MWBot { get; set; }
    public int CarId { get; set; }
    public Car Car { get; set; }
    public string UserId { get; set; }
    public IdentityUser User { get; set; }
}
