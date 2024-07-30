using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Pages
{
    public class MonitoringParkingModel : PageModel
    {
        private readonly ILogger<MonitoringParkingModel> _logger;
        private readonly ParkingSlotRepository _parkingSlotRepository;
        private readonly ParkingRepository _parkingRepository;

        public MonitoringParkingModel(ILogger<MonitoringParkingModel> logger, ParkingSlotRepository repository, ParkingRepository parkingRepository)
        {
            _logger = logger;
            _parkingSlotRepository = repository;
            _parkingRepository = parkingRepository;
        }

        public IList<ParkingSlotViewModel> ParkingSlots { get; private set; }

        [BindProperty]
        public List<Parking> Parkings { get; set; } = new();

        public async Task OnGetAsync(
            [FromQuery] string? searchSlotNumber,
            [FromQuery] ParkingSlotStatus? parkingSlotStatus,
            [FromQuery] int? parkingSlotId)
        {
            var parkingSlots = await _parkingSlotRepository.GetFilteredAsync(searchSlotNumber, parkingSlotStatus, parkingSlotId);
            ParkingSlots = parkingSlots.Select(slot => new ParkingSlotViewModel
            {
                Id = slot.Id,
                Number = slot.Number,
                Status = slot.Status,
                Parking = _parkingRepository.GetParkingById(slot.ParkingId),
                StatusColor = GetStatusColor(slot.Status)
            }).ToList();

            Parkings = await _parkingRepository.GetAllAsync();


        }

        private string GetStatusColor(ParkingSlotStatus status)
        {
            return status switch
            {
                ParkingSlotStatus.Free => "green",
                ParkingSlotStatus.Occupied => "red",
                ParkingSlotStatus.Reserved => "orange",
                ParkingSlotStatus.OutOfService => "grey",
                _ => "black"
            };
        }
    }

    public class ParkingSlotViewModel : ParkingSlot
    {
        public string StatusColor { get; set; }
    }
}