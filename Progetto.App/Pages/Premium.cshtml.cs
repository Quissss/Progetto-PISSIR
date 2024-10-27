using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayPal.REST.Client;
using PayPal.REST.Models.Orders;
using PayPal.REST.Models.PaymentSources;
using System.Globalization;

namespace Progetto.App.Pages
{
    public class PremiumModel : PageModel
    {
        private readonly ILogger<PremiumModel> _logger;
        private readonly IPayPalClient _payPalClient;

        public PremiumModel(ILogger<PremiumModel> logger, IPayPalClient payPalClient)
        {
            _logger = logger;
            _payPalClient = payPalClient;
        }

        public void OnGet()
        {

        }

        public async Task OnGetSuccess(string token, string payerId)
        {
            //Success
            await _payPalClient.CapturePayment(token);
            using var client = new HttpClient { BaseAddress = new("https://localhost:7237") };
            var res = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/api/user/upgrade"));
        }

        public async Task OnGetCancel(string token, string? payerId)
        {
            //Cancel
        }

        public async Task<IActionResult> OnPostPay([FromForm] string plan)
        {
            _logger.LogDebug("Updating user to premium");
            decimal cost = 5.0M;
            switch (plan)
            {
                case "Base":
                    break;
                case "Standard":
                    cost = 10;
                    break;
                case "Premium":
                    cost = 15;
                    break;
                default:
                    break;
            }
            var orderRequest = new OrderRequest
            {
                PurchaseUnits = new()
        {
            new()
            {
                Items = new()
                {
                    new()
                    {
                        Name = "User upgrade",
                        Description = $"Update for {plan} subscription plan",
                        Quantity = "1",
                        UnitAmount = new()
                        {
                            Value = cost.ToString(CultureInfo.InvariantCulture)
                        },
                        Category = ItemCategory.DigitalGoods
                    }
                },
                Amount = new()
                {
                    Value = cost.ToString(CultureInfo.InvariantCulture),
                    Breakdown = new()
                    {
                        ItemTotal = new()
                        {
                            Value = cost.ToString(CultureInfo.InvariantCulture)
                        }
                    }
                }
            }
        }
            };

            var response = await _payPalClient.CreateOrder(orderRequest);
            var paymentSource = new PayPalPaymentSource
            {
                PayPalData = new()
                {
                    ExperienceContext = new()
                    {
                        LandingPage = LandingPage.Login,
                        PaymentMethodPreference = PaymentMethodPreference.ImmediatePaymentRequired,
                        UserAction = UserAction.PayNow,
                        BrandName = "EcoPlug & Go",
                        ReturnUrl = $"https://localhost:7237/Premium?handler=success",
                        CancelUrl = "https://localhost:7237/Premium?handler=cancel"
                    }
                }
            };

            response = await _payPalClient.ConfirmOrder(response.Id, paymentSource);
            var checkoutUrl = response.Links.Where(l => l.Method.ToUpperInvariant() == "GET" && l.Rel.ToLowerInvariant() == "payer-action").First();

            return Redirect(checkoutUrl.Href);
        }
    }
}
