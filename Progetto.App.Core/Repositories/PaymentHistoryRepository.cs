using Microsoft.EntityFrameworkCore;
using Progetto.App.Core.Data;
using Progetto.App.Core.Models;

namespace Progetto.App.Core.Repositories;

/// <summary>
/// Repository for PaymentHistory entity (inherits from GenericRepository)
/// For database operations related to PaymentHistory entity
/// </summary>
public class PaymentHistoryRepository : GenericRepository<PaymentHistory>
{
    private readonly ApplicationDbContext _context;

    public PaymentHistoryRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<PaymentHistory>> GetPaymentsWithinDateRange(DateTime startDate, DateTime endDate)
    {
        return await _context.PaymentHistory
                             .Where(p => p.StartTime >= startDate && p.EndTime <= endDate)
                             .ToListAsync();
    }

    public async Task<List<PaymentHistory>> GetPaymentsWithinDateRangeAndType(DateTime startDate, DateTime endDate, bool? chargeType)
    {
        var query = _context.PaymentHistory
                            .Where(p => p.StartTime >= startDate && p.EndTime <= endDate);

        if (chargeType.HasValue)
        {
            query = query.Where(p => p.IsCharge == chargeType.Value);
        }

        return await query.ToListAsync();
    }

}
