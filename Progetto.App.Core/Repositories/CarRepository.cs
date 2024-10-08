using Microsoft.EntityFrameworkCore;
using Progetto.App.Core.Data;
using Progetto.App.Core.Models;

namespace Progetto.App.Core.Repositories;

/// <summary>
/// Repository for Car entity (inherits from GenericRepository)
/// For database operations related to Car entity
/// </summary>
public class CarRepository : GenericRepository<Car>
{
    private readonly ApplicationDbContext _context;

    public CarRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Car?> GetCarByPlate(string licencePlate)
    {
        return await _context.Cars.Where(c => c.LicencePlate == licencePlate).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Car?>> GetCarsByOwner(string ownerId)
    {
        return await _context.Cars.Where(c => c.OwnerId == ownerId).ToListAsync();
    }

    public async Task<Car> UpdateCarStatus(string licencePlate, CarStatus status, int? parkingSlotId = null)
    {
        var car = await _context.Cars.Where(c => c.LicencePlate == licencePlate).FirstAsync();
        car.Status = status;
        if (status == CarStatus.InCharge)
        {
            car.ParkingSlotId = parkingSlotId;
        }

        await _context.SaveChangesAsync();
        return car;
    }
}
