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
/// Repository for Parking entity (inherits from GenericRepository)
/// For database operations related to Parking entity
/// </summary>
public class ParkingRepository : GenericRepository<Parking>
{
    private readonly ApplicationDbContext _context;

    public ParkingRepository(ApplicationDbContext context) : base(context) 
    {
        _context = context;
    }

    public async Task<Parking?> GetByParkingSlotId(int parkingSlotId)
    {
        return await _context.Parkings.Where(p => p.ParkingSlots.Any(ps => ps.Id == parkingSlotId)).FirstOrDefaultAsync();
    }

    public async Task<Parking?> GetParkingByName(string name)
    {
        return await _context.Parkings.Where(p => p.Name == name).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Parking?>> GetParkingsByCity(string city)
    {
        return await _context.Parkings.Where(p => p.City == city).ToListAsync();
    }

    public async Task<IEnumerable<Parking?>> GetParkingsByProvince(string province)
    {
        return await _context.Parkings.Where(p => p.Province == province).ToListAsync();
    }

    public async Task<IEnumerable<Parking?>> GetParkingsByCountry(string country)
    {
        return await _context.Parkings.Where(p => p.Country == country).ToListAsync();
    }

    public async Task<IEnumerable<Parking?>> GetParkingsByPostalCode(string postalCode)
    {
        return await _context.Parkings.Where(p => p.PostalCode == postalCode).ToListAsync();
    }

    public  Parking? GetParkingById(int parkingId)
    {
        return  _context.Parkings.Where(p => p.Id == parkingId).FirstOrDefault();
    }


}
