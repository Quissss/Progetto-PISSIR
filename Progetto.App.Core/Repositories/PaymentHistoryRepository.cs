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
}
