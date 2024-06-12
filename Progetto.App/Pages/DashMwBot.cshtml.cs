using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Pages
{
    public class DashMwBotModel : PageModel
{
    private readonly MwBotRepository _mwBotRepository;
    private readonly ILogger<DashMwBotModel> _logger;

    public DashMwBotModel(MwBotRepository mwBotRepository, ILogger<DashMwBotModel> logger)
    {
        _mwBotRepository = mwBotRepository;
        _logger = logger;
    }

    public IEnumerable<MwBot> MwBots { get; private set; }

    public async Task OnGet()
    {
        _logger.LogDebug("Requested page {path}", Request.Path);

        MwBots = await _mwBotRepository.GetAllAsync();
    }
}
}
