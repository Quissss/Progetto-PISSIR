using Microsoft.EntityFrameworkCore;
using Progetto.App.Core.Data;
using Progetto.App.Core.Models;

namespace Progetto.App.Core.Repositories;

/// <summary>
/// Repository for Reservation entity (inherits from GenericRepository)
/// For database operations related to Reservation entity
/// </summary>
public class ReservationRepository : GenericRepository<Reservation>
{
    private readonly ApplicationDbContext _context;

    public ReservationRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Reservation?>> GetByUserId(string userId)
    {
        return await _context.Reservations.Where(r => r.UserId == userId).ToListAsync();
    }

    public async Task<IEnumerable<Reservation?>> GetByCarPlate(string plate)
    {
        return await _context.Reservations.Where(r => r.CarPlate == plate).ToListAsync();
    }

    public async Task<IEnumerable<Reservation?>> UpdateCarIsInside(string plate, int parkingId, bool carInside)
    {
        var reservations = await _context.Reservations.Where(r => r.CarPlate == plate && r.ParkingId == parkingId).ToListAsync();
        reservations.ForEach(r => r.CarIsInside = carInside);
        await _context.SaveChangesAsync();
        return reservations;
    }
}
