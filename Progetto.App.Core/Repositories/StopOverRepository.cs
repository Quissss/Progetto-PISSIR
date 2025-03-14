﻿using Microsoft.EntityFrameworkCore;
using Progetto.App.Core.Data;
using Progetto.App.Core.Models;

namespace Progetto.App.Core.Repositories;

/// <summary>
/// Repository for Stopover entity (inherits from GenericRepository)
/// For database operations related to Stopover entity
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

    public async Task<Stopover?> GetFirstByCarPlate(string carPlate)
    {
        return await _context.Stopovers
            .Where(s => s.CarPlate == carPlate && !s.ToPay)
            .OrderBy(s => s.StartStopoverTime)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Stopover?>> GetByUserId(string userId)
    {
        return await _context.Stopovers
            .Where(s => s.UserId == userId).ToListAsync();
    }
}
