using Microsoft.EntityFrameworkCore;
using Progetto.App.Core.Data;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Repositories;

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

    public async Task<IEnumerable<ParkingSlot>> GetFreeParkingSlots()
    {
        return await _context.ParkingSlots.Where(ps => ps.Status == ParkSlotStatus.Free).ToListAsync();
    }

    public async Task<IEnumerable<ParkingSlot>> GetOccupiedParkingSlots()
    {
        return await _context.ParkingSlots.Where(ps => ps.Status == ParkSlotStatus.Occupied).ToListAsync();
    }
}
