using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models;

public class Car
{
    public string LicencePlate { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string OwnerId { get; set; }
    public IdentityUser Owner { get; set; }
}
