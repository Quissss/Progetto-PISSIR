using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Pages
{
    public class DashParkingModel : PageModel
    {
        private readonly ParkingSlotRepository _slotRepository;
        private readonly ParkingRepository _parkingRepository;
        private readonly ILogger<DashParkingModel> _logger;

        public DashParkingModel(ParkingSlotRepository parkingSlotRepository, ParkingRepository parkingRepository, ILogger<DashParkingModel> logger)
        {
            _slotRepository = parkingSlotRepository;
            _parkingRepository = parkingRepository;
            _logger = logger;
        }

        public IEnumerable<SelectListItem> Parkings { get; set; }

        public async Task OnGet()
        {
            _logger.LogDebug("Requested page {path}", Request.Path);

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
