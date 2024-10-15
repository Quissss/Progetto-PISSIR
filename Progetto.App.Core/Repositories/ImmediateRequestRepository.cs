using Microsoft.EntityFrameworkCore;
using Progetto.App.Core.Data;
using Progetto.App.Core.Models;

namespace Progetto.App.Core.Repositories;

/// <summary>
/// Repository for ImmediateRequest entity (inherits from GenericRepository)
/// For database operations related to ImmediateRequest entity
/// </summary>
public class ImmediateRequestRepository : GenericRepository<ImmediateRequest>
{
    private readonly ApplicationDbContext _context;

    public ImmediateRequestRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ImmediateRequest?>> GetUnhandledWithNoReservation()
    {
        return await _context.ImmediateRequests
            .Where(ir => ir.FromReservation == false && ir.IsBeingHandled == false)
            .ToListAsync();
    }

    public async Task<ImmediateRequest?> GetByCarPlate(string carPlate)
    {
        return await _context.ImmediateRequests
            .Where(ir => ir.CarPlate == carPlate)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteByCarPlate(string licencePlate)
    {
        var immediateRequest = await _context.ImmediateRequests
            .Where(ir => ir.CarPlate == licencePlate)
            .FirstOrDefaultAsync();

        if (immediateRequest != null)
        {
            _context.ImmediateRequests.Remove(immediateRequest);
            await _context.SaveChangesAsync();
        }
    }
}
