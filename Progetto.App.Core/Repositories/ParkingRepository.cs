using Microsoft.EntityFrameworkCore;
using Progetto.App.Core.Data;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Repositories;

public class ParkingRepository : GenericRepository<Parking>
{
    public ParkingRepository(ApplicationDbContext context) : base(context) { }
 
}

