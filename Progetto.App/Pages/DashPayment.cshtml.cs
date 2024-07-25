using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Progetto.App.Pages
{
    public class DashPaymentModel : PageModel
    {
        private readonly CurrentlyChargingRepository _currentlyChargingRepository;
        private readonly ILogger<DashPaymentModel> _logger;

        public DashPaymentModel(CurrentlyChargingRepository currentlyChargingRepository, ILogger<DashPaymentModel> logger)
        {
            _currentlyChargingRepository = currentlyChargingRepository;
            _logger = logger;
        }

        [BindProperty]
        public DateTime? StartDate { get; set; }

        [BindProperty]
        public DateTime? EndDate { get; set; }

        public List<CurrentlyCharging> Payments { get; set; } = new List<CurrentlyCharging>();

        public async Task<IActionResult> OnPostAsync()
        {
            if (StartDate == null || EndDate == null)
            {
                ModelState.AddModelError(string.Empty, "Please select a valid date range.");
                return Page();
            }

            Payments = await _currentlyChargingRepository.GetPaymentsWithinDateRange(StartDate.Value, EndDate.Value);

            return Page();
        }

        public async Task OnGetAsync()
        {
            // Optionally initialize with some default values
            StartDate = DateTime.Now.AddDays(-7); // last 7 days
            EndDate = DateTime.Now;

            Payments = await _currentlyChargingRepository.GetPaymentsWithinDateRange(StartDate.Value, EndDate.Value);
        }
    }
}
