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
public class CurrentlyChargingRepository : GenericRepository<CurrentlyCharging>
{
    private readonly ApplicationDbContext _context;

    public CurrentlyChargingRepository(ApplicationDbContext context) : base(context) 
    {
        _context = context;
    }

    public async Task<CurrentlyCharging?> GetByMwBotId(int mwBotId)
    {
        return await _context.CurrentlyCharging
            .Where(c => c.MwBotId == mwBotId)
            .FirstOrDefaultAsync();
    }

    public async Task<CurrentlyCharging?> GetCurrentlyChargingByCarId(string carPlate)
    {
        return await _context.CurrentlyCharging
            .Where(c => c.CarPlate == carPlate)
            .FirstOrDefaultAsync();
    }

    public async Task<CurrentlyCharging?> GetByCurrentlyChargingMwBot(int mwBotId)
    {
        return await _context.CurrentlyCharging
            .Where(c => c.MwBotId == mwBotId && c.EndChargingTime == null)
            .FirstOrDefaultAsync();
    }

    public async Task<CurrentlyCharging?> GetChargingCars()
    {
        return await _context.CurrentlyCharging
            .Where(c => !string.IsNullOrEmpty(c.CarPlate) && c.ParkingSlotId != 0 && !string.IsNullOrEmpty(c.UserId))
            .FirstOrDefaultAsync();
    }

    public async Task<CurrentlyCharging?> GetChargingBots()
    {
        return await _context.CurrentlyCharging
            .Where(c => string.IsNullOrEmpty(c.CarPlate))
            .FirstOrDefaultAsync();
    }
}
