using Microsoft.EntityFrameworkCore;
using Progetto.App.Core.Data;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Repositories;

public class MwBotRepository : GenericRepository<MwBot>
{
    private readonly ApplicationDbContext _context;

    public MwBotRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MwBot>> GetOnlineMwBots()
    {
        return await _context.MwBots.Where(m => m.Status != MwBotStatus.Offline).ToListAsync();
    }

    public async Task<IEnumerable<MwBot>> GetOfflineMwBots()
    {
        return await _context.MwBots.Where(m => m.Status == MwBotStatus.Offline).ToListAsync();
    }
}
