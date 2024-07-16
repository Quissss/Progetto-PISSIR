﻿using Progetto.App.Core.Data;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Repositories;

/// <summary>
/// Repository for ChargeHistory entity (inherits from GenericRepository)
/// For database operations related to ChargeHistory entity
/// </summary>
public class ChargeHistoryRepository : GenericRepository<ChargeHistory>
{
    private readonly ApplicationDbContext _context;

    public ChargeHistoryRepository(ApplicationDbContext context) : base(context) 
    {
        _context = context;
    }
}
