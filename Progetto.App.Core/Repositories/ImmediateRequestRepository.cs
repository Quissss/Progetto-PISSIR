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
}
