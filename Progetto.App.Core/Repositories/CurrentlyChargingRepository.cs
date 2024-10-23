using Microsoft.EntityFrameworkCore;
using Progetto.App.Core.Data;
using Progetto.App.Core.Models;

namespace Progetto.App.Core.Repositories;

/// <summary>
/// Repository for CurrentlyCharging entity (inherits from GenericRepository)
/// For database operations related to CurrentlyCharging entity
/// </summary>
public class CurrentlyChargingRepository : GenericRepository<CurrentlyCharging>
{
    private readonly ApplicationDbContext _context;

    public CurrentlyChargingRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<CurrentlyCharging?> GetByActiveMwBotId(int mwBotId)
    {
        return await _context.CurrentlyCharging
            .Where(c => c.MwBotId == mwBotId && c.EndChargingTime == null && !c.ToPay && c.CurrentChargePercentage <= c.TargetChargePercentage && c.ImmediateRequestId != null)
            .OrderBy(c => c.StartChargingTime)
            .FirstOrDefaultAsync();
    }

    public async Task<CurrentlyCharging?> GetChargingByCarPlate(string carPlate)
    {
        return await _context.CurrentlyCharging
            .Where(c => c.CarPlate == carPlate && c.EndChargingTime == null && !c.ToPay && c.CurrentChargePercentage <= c.TargetChargePercentage && c.ImmediateRequestId != null)
            .OrderBy(c => c.StartChargingTime)
            .FirstOrDefaultAsync();
    }

    public async Task<CurrentlyCharging?> GetActiveWithNoBotByParking(int parkingId)
    {
        return await _context.CurrentlyCharging
            .Include(cc => cc.ParkingSlot)
            .Where(c => c.MwBotId == null && c.EndChargingTime == null && !c.ToPay && c.CurrentChargePercentage <= c.TargetChargePercentage && c.ImmediateRequestId != null && c.ParkingSlot.ParkingId == parkingId)
            .OrderBy(c => c.StartChargingTime)
            .FirstOrDefaultAsync();
    }

    public async Task<CurrentlyCharging?> GetActiveByMwBot(int mwBotId)
    {
        return await _context.CurrentlyCharging
            .Where(c => c.MwBotId == mwBotId && c.EndChargingTime == null && !c.ToPay && c.CurrentChargePercentage <= c.TargetChargePercentage && c.ImmediateRequestId != null)
            .OrderBy(c => c.StartChargingTime)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CurrentlyCharging?>> GetByUserId(string userId)
    {
        return await _context.CurrentlyCharging
            .Where(c => c.UserId == userId).ToListAsync();
    }
}
