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
/// Repository for MwBot entity (inherits from GenericRepository)
/// For database operations related to MwBot entity
/// </summary>
public class MwBotRepository : GenericRepository<MwBot>
{
    private readonly ApplicationDbContext _context;

    public MwBotRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public new async Task UpdateAsync(MwBot mwBot)
    {
        _context.MwBots.Update(mwBot);
        await _context.SaveChangesAsync();
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
