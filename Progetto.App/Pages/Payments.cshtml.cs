using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Pages
{
    public class PaymentsModel : PageModel
    {
        private readonly CurrentlyChargingRepository _currentlyChargingRepository;

        public PaymentsModel(CurrentlyChargingRepository currentlyChargingRepository)
        {
            _currentlyChargingRepository = currentlyChargingRepository;
        }

        public List<CurrentlyCharging> CurrentCharges { get; private set; }

        public async Task OnGetAsync()
        {
            CurrentCharges = await _currentlyChargingRepository.GetAllAsync();
        }
    }
}