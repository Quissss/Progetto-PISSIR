using Microsoft.EntityFrameworkCore;
using Progetto.App.Core.Data;
using Progetto.App.Core.Models;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public async Task<Car?> GetCarByLicencePlate(string licencePlate)
    {
        return await _context.Cars.Where(c => c.LicencePlate == licencePlate).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Car?>> GetCarsByOwner(string ownerId)
    {
        return await _context.Cars.Where(c => c.OwnerId == ownerId).ToListAsync();
    }
}
