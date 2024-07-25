using Microsoft.EntityFrameworkCore;
using Progetto.App.Core.Data;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Repositories;

/// <summary>
/// Repository for StopoverHistory entity (inherits from GenericRepository)
/// For database operations related to StopoverHistory entity
/// </summary>
public class StopoverHistoryRepository : GenericRepository<StopoverHistory>
{
    private readonly ApplicationDbContext _context;

    public StopoverHistoryRepository(ApplicationDbContext context) : base(context) 
    {
        _context = context;
    }
}
