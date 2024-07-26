using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using PayPal.REST.Client;
using PayPal.REST.Models.Orders;
using PayPal.REST.Models.PaymentSources;
using Microsoft.AspNetCore.Identity;

namespace Progetto.App.Pages;

public class PaymentsModel : PageModel
{
    private readonly CurrentlyChargingRepository _currentlyChargingRepository;
    private readonly PayPalClient _payPalClient;
    private readonly StopoverRepository _stopoverRepository;
    private readonly UserManager<IdentityUser> _userManager;

    public PaymentsModel(
        CurrentlyChargingRepository currentlyChargingRepository, 
        StopoverRepository stopoverRepository,
        UserManager<IdentityUser> userManager
        )
    {
        _currentlyChargingRepository = currentlyChargingRepository;
        _payPalClient = new PayPalClient("tuoClientId", "tuoClientSecret", "https://api.sandbox.paypal.com"); // Configura con le tue credenziali PayPal
        _stopoverRepository = stopoverRepository;
        _userManager = userManager;
    }

    public List<CurrentlyCharging> CurrentCharges { get; private set; }
    public List<Stopover> StopCharges { get; private set; }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        CurrentCharges = (await _currentlyChargingRepository.GetByUserId(user.Id)).ToList();
        StopCharges = (await _stopoverRepository.GetByUserId(user.Id)).ToList();
    }

    public async Task<IActionResult> OnPostPayNowAsync(string carPlate)
    {
        // Log the request
        Console.WriteLine($"Received payment request for car plate: {carPlate}");

        // Trova il dettaglio del pagamento basato sulla carPlate
        var charge = CurrentCharges.FirstOrDefault(c => c.CarPlate == carPlate);
        if (charge == null)
        {
            Console.WriteLine("Charge not found.");
            return NotFound(new { success = false, message = "Charge not found." });
        }

        var orderRequest = new OrderRequest
        {
            // Configura i dettagli del tuo ordine qui
        };

        try
        {
            var orderResponse = await _payPalClient.CreateOrder(orderRequest);
            var paymentSource = new PayPalPaymentSource
            {
                // Configura i dettagli della fonte di pagamento qui
            };

            var confirmedOrder = await _payPalClient.ConfirmOrder(orderResponse.Id, paymentSource);
            var capturedOrder = await _payPalClient.CapturePayment(confirmedOrder.Id);

            return new JsonResult(new { success = true, message = "Payment completed successfully!" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }
}
