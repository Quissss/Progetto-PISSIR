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

    public async Task<CurrentlyCharging?> GetActiveByImmediateRequest(int immediateRequestId)
    {
        return await _context.CurrentlyCharging
            .Where(c => c.ImmediateRequestId == immediateRequestId && c.EndChargingTime == null && !c.ToPay && c.CurrentChargePercentage < c.TargetChargePercentage)
            .FirstOrDefaultAsync();
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

    public async Task<CurrentlyCharging?> GetActiveByMwBot(int mwBotId)
    {
        return await _context.CurrentlyCharging
            .Where(c => c.MwBotId == mwBotId && c.EndChargingTime == null && !c.ToPay && c.CurrentChargePercentage < c.TargetChargePercentage)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CurrentlyCharging?>> GetAllActiveByMwBot(int mwBotId)
    {
        return await _context.CurrentlyCharging
            .Where(c => c.MwBotId == mwBotId && c.EndChargingTime == null && !c.ToPay && c.CurrentChargePercentage < c.TargetChargePercentage)
            .ToListAsync();
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

    public async Task<IEnumerable<CurrentlyCharging?>> GetByUserId(string userId)
    {
        return await _context.CurrentlyCharging
            .Where(c => c.UserId == userId).ToListAsync();
    }

    public async Task<CurrentlyCharging?> GetByImmediateRequestId(int immediateRequestId)
    {
        return await _context.CurrentlyCharging
            .Where(c => c.ImmediateRequestId == immediateRequestId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<CurrentlyCharging>> GetPaymentsWithinDateRange(DateTime startDate, DateTime endDate)
    {
        return await _context.CurrentlyCharging
            .Where(c => c.StartChargingTime >= startDate && c.EndChargingTime <= endDate)
            .OrderBy(c => c.StartChargingTime)
            .ToListAsync();
    }

}
