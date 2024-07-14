using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task OnGetAsync()
        {
            var parkingSlots = await _parkingSlotRepository.GetAllAsync();
            ParkingSlots = parkingSlots.Select( slot => new ParkingSlotViewModel
            {
                Id = slot.Id,
                Number = slot.Number,
                Status = slot.Status,
                Parking =  _parkingRepository.GetParkingById(slot.ParkingId),
                StatusColor = GetStatusColor(slot.Status)
            }).ToList();
        }

        private string GetStatusColor(ParkSlotStatus status)
        {
            return status switch
            {
                ParkSlotStatus.Free => "green",
                ParkSlotStatus.Occupied => "red",
                ParkSlotStatus.Reserved => "orange",
                ParkSlotStatus.OutOfService => "grey",
                _ => "black"
            };
        }
    }

    public class ParkingSlotViewModel : ParkingSlot
    {
        public string StatusColor { get; set; }
    }
}