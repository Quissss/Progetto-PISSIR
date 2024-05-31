﻿using Progetto.App.Core.Data;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Repositories;

public class ParkingSlotRepository : GenericRepository<ParkingSlot>
{
    public ParkingSlotRepository(ApplicationDbContext context) : base(context) { }
}
