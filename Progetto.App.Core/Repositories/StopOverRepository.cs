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
/// Repository for ChargeHistory entity (inherits from GenericRepository)
/// For database operations related to ChargeHistory entity
/// </summary>
public class StopoverRepository : GenericRepository<Stopover>
{
    private readonly ApplicationDbContext _context;

    public StopoverRepository(ApplicationDbContext context) : base(context) 
    {
        _context = context;
    }

    public async Task<IEnumerable<Stopover?>> GetAllByCarPlate(string carPlate)
    {
        return await _context.Stopovers.Where(s => s.CarPlate == carPlate).ToListAsync();
    }

    public async Task<Stopover?>  GetFirstByCarPlate(string carPlate)
    {
        return await _context.Stopovers
            .Where(s => s.CarPlate == carPlate && !s.ToPay)
            .OrderBy(s => s.StartStopoverTime)
            .FirstOrDefaultAsync();
    }
}
