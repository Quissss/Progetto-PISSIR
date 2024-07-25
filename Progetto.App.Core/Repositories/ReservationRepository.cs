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

    public async Task<IEnumerable<Reservation?>> GetByCarPlate(string LicencePlate)
    {
        return await _context.Reservations.Where(r => r.CarPlate == LicencePlate).ToListAsync();
    }

    public async Task<IEnumerable<Reservation?>> UpdateCarIsInside(string LicencePlate, int ParkingId, bool CarInside)
    {
        var reservations = await _context.Reservations.Where(r => r.CarPlate == LicencePlate && r.ParkingId == ParkingId).ToListAsync();
        reservations.ForEach(r => r.CarIsInside = CarInside);
        await _context.SaveChangesAsync();
        return reservations;
    }
}
