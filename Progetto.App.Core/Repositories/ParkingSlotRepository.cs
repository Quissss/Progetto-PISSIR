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

    public async Task<IEnumerable<ParkingSlot>> GetParkingSlotsByParking(int parkingId)
    {
        return await _context.ParkingSlots.Where(ps => ps.ParkingId == parkingId).ToListAsync();
    }

    public async Task<ParkingSlot?> GetParkingSlotByNumber(int parkingId, int number)
    {
        return await _context.ParkingSlots.Where(ps => ps.ParkingId == parkingId && ps.Number == number).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ParkingSlot>> GetFreeParkingSlots(int? parkingId)
    {
        return await _context.ParkingSlots.Where(ps => ps.Status == ParkSlotStatus.Free && ps.ParkingId == parkingId).ToListAsync();
    }

    public async Task<IEnumerable<ParkingSlot>> GetOccupiedParkingSlots(int? parkingId)
    {
        return await _context.ParkingSlots.Where(ps => ps.Status == ParkSlotStatus.Occupied && ps.ParkingId == parkingId).ToListAsync();
    }

    public async Task<IEnumerable<ParkingSlot>> GetFilteredAsync(string searchSlotNumber, ParkSlotStatus? parkingSlotStatus, int? parkingSlotId)
    {
        IQueryable<ParkingSlot> query = _context.ParkingSlots;

        if (!string.IsNullOrEmpty(searchSlotNumber))
        {
            query = query.Where(ps => ps.Number.ToString().Contains(searchSlotNumber));
        }

        if (parkingSlotStatus.HasValue)
        {
            query = query.Where(ps => ps.Status == parkingSlotStatus.Value);
        }

        if (parkingSlotId.HasValue)
        {
            query = query.Where(ps => ps.ParkingId == parkingSlotId.Value);
        }

        return await query.ToListAsync();
    }

}
