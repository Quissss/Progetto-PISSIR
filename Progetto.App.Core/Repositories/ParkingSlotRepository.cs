using Microsoft.EntityFrameworkCore;
using Progetto.App.Core.Data;
using Progetto.App.Core.Models;

namespace Progetto.App.Core.Repositories;

/// <summary>
/// Repository for ParkingSlot entity (inherits from GenericRepository)
/// For database operations related to ParkingSlot entity
/// </summary>
public class ParkingSlotRepository : GenericRepository<ParkingSlot>
{
    private readonly ApplicationDbContext _context;

    public ParkingSlotRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ParkingSlot>> GetFreeParkingSlots(int? parkingId)
    {
        return await _context.ParkingSlots.Where(ps => ps.Status == ParkingSlotStatus.Free && ps.ParkingId == parkingId).ToListAsync();
    }

    public async Task<ParkingSlot?> GetFreeParkingSlot(int? parkingId)
    {
        return await _context.ParkingSlots.Where(ps => ps.Status == ParkingSlotStatus.Free && ps.ParkingId == parkingId).FirstOrDefaultAsync();
    }
}
