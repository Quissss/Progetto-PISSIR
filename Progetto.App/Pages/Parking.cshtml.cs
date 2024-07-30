using Microsoft.AspNetCore.Mvc.RazorPages;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Pages
{
    public class ParkingModel : PageModel
    {
        private readonly ParkingRepository _parkingRepository;
        private readonly ILogger<ParkingModel> _logger;

        public ParkingModel(ParkingRepository parkingRepository, ILogger<ParkingModel> logger)
        {
            _parkingRepository = parkingRepository;
            _logger = logger;
        }

        public IEnumerable<Parking> Parkings { get; set; }

        public async Task OnGet()
        {
            _logger.LogDebug("Requested page {path}", Request.Path);

            Parkings = await _parkingRepository.GetAllAsync();
        }
    }
}
