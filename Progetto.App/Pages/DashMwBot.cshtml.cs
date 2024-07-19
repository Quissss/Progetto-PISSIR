using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Pages
{
    public class DashMwBotModel : PageModel
    {
        private readonly MwBotRepository _mwBotRepository;
        private readonly ILogger<DashMwBotModel> _logger;
        private readonly ParkingRepository _parkingRepository;

        public DashMwBotModel(MwBotRepository mwBotRepository, ILogger<DashMwBotModel> logger, ParkingRepository parkingRepository)
        {
            _mwBotRepository = mwBotRepository;
            _logger = logger;
            _parkingRepository = parkingRepository;
        }
      
        public IEnumerable<MwBot> MwBots { get; private set; }
        public IEnumerable<SelectListItem> Parkings { get; set; }


        public async Task OnGet()
        {
            _logger.LogDebug("Requested page {path}", Request.Path);

            MwBots = await _mwBotRepository.GetAllAsync();

            var parkings = await _parkingRepository.GetAllAsync();
            List<SelectListItem> ps = new();
            foreach (var p in parkings)
            {
                ps.Add(new SelectListItem { Text = $"{p.Name} [{p.City} - {p.Address}]", Value = p.Id.ToString() });
            }
            Parkings = ps;

        }
    }
}
