using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Pages
{
    public class DashPaymentModel : PageModel
    {
        private readonly PaymentHistoryRepository _paymentHistoryRepository;
        private readonly ILogger<DashPaymentModel> _logger;

        public DashPaymentModel(PaymentHistoryRepository paymentHistoryRepository, ILogger<DashPaymentModel> logger)
        {
            _paymentHistoryRepository = paymentHistoryRepository;
            _logger = logger;
        }

        [BindProperty]
        public DateTime? StartDate { get; set; }

        [BindProperty]
        public DateTime? EndDate { get; set; }

        [BindProperty]
        public bool? ChargeType { get; set; }

        public List<PaymentHistory> Payments { get; set; } = new List<PaymentHistory>();

        public async Task<IActionResult> OnPostAsync()
        {
            if (StartDate == null || EndDate == null)
            {
                ModelState.AddModelError(string.Empty, "Please select a valid date range.");
                return Page();
            }

            Payments = await _paymentHistoryRepository.GetPaymentsWithinDateRangeAndType(StartDate.Value, EndDate.Value, ChargeType);

            return Page();
        }

        public async Task OnGetAsync()
        {
            // Optionally initialize with some default values
            StartDate = DateTime.Now.AddDays(-7); // last 7 days
            EndDate = DateTime.Now;

            Payments = await _paymentHistoryRepository.GetPaymentsWithinDateRangeAndType(StartDate.Value, EndDate.Value, ChargeType);
        }
    }
}
