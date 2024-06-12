﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models;

/// <summary>
/// Represents a parking
/// </summary>
public class Parking
{
    public int Id { get; set; }
    public decimal EnergyCostPerKw { get; set; }
    public decimal StopCostPerMinute { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public List<ParkingSlot>? ParkingSlots { get; set; }
}
