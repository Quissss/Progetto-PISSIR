using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PayPal.REST.Client;
using PayPal.REST.Models.Orders;
using PayPal.REST.Models.PaymentSources;
using Progetto.App.Core.Repositories;
using System.Dynamic;
using System.Globalization;

namespace Progetto.App.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly PaymentHistoryRepository _paymentHistoryRepository;
    private readonly IPayPalClient _payPalClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PaymentController(PaymentHistoryRepository paymentHistoryRepository, IServiceScopeFactory serviceScopeFactory, IPayPalClient payPalClient)
    {
        _paymentHistoryRepository = paymentHistoryRepository;
        _serviceScopeFactory = serviceScopeFactory;
        _payPalClient = payPalClient;
    }

    [HttpPost("checkout")]
    public async Task<ActionResult> Checkout([FromBody] dynamic body)
    {
        // Deserializza il body in un oggetto JSON dinamico
        var jsonObject = JsonConvert.DeserializeObject<ExpandoObject>(body.ToString());

        // Converte l'ExpandoObject in un dizionario per un facile accesso
        var dict = (IDictionary<string, object>)jsonObject;

        // Ottiene i valori di id e isCharge dal dizionario
        int id = Convert.ToInt32(dict["id"]);
        bool isCharge = Convert.ToBoolean(dict["isCharge"]);

        if (id == 0)
        {
            return BadRequest("Invalid id");
        }

        var orderRequest = new OrderRequest();

        if (isCharge)
        {
            var chargeRepository = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();
            var charge = await chargeRepository.GetByIdAsync(id);

            orderRequest.PurchaseUnits = new()
            {
                new()
                {
                    Items = new()
                    {
                        new()
                        {
                            Name = "Charge",
                            Description = $"Charge payment for {charge.EnergyConsumed} kW",
                            Quantity = "1",
                            UnitAmount = new()
                            {
                                Value = charge.TotalCost.ToString(CultureInfo.InvariantCulture)
                            },
                            Category = ItemCategory.DigitalGoods
                        }
                    },
                    Amount = new()
                    {
                        Value = charge.TotalCost.ToString(CultureInfo.InvariantCulture),
                        Breakdown = new()
                        {
                            ItemTotal = new()
                            {
                                Value = charge.TotalCost.ToString(CultureInfo.InvariantCulture)
                            }
                        }
                    }
                }
            };
        }
        else
        {
            var stopoverRepository = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<StopoverRepository>();
            var stopover = await stopoverRepository.GetByIdAsync(id);

            var duration = stopover.EndStopoverTime.Value - stopover.StartStopoverTime.Value;
            var totalMinutes = duration.TotalMinutes;

            orderRequest.PurchaseUnits = new()
            {
                new()
                {
                    Items = new()
                    {
                        new()
                        {
                            Name = "Stopover",
                            Description = $"Stopover payment for {totalMinutes} minutes",
                            Quantity = "1",
                            UnitAmount = new()
                            {
                                Value = stopover.TotalCost.ToString(CultureInfo.InvariantCulture)
                            },
                            Category = ItemCategory.DigitalGoods
                        }
                    },
                    Amount = new()
                    {
                        Value = stopover.TotalCost.ToString(CultureInfo.InvariantCulture),
                        Breakdown = new()
                        {
                            ItemTotal = new()
                            {
                                Value = stopover.TotalCost.ToString(CultureInfo.InvariantCulture)
                            }
                        }
                    }
                }
            };
        }

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
                    ReturnUrl = $"https://localhost:7237/api/payment/complete?id={id}",
                    CancelUrl = "https://localhost:7237/api/payment/cancel"
                }
            }
        };

        response = await _payPalClient.ConfirmOrder(response.Id, paymentSource);
        var checkoutUrl = response.Links.Where(l => l.Method.ToUpperInvariant() == "GET" && l.Rel.ToLowerInvariant() == "payer-action").First();

        return Ok(new { Url = checkoutUrl.Href });
    }

    [AllowAnonymous]
    [HttpGet("complete")]
    public async Task<ActionResult> Complete(int id, string token, string payerId)
    {
        var ok = Request.GetEncodedUrl();
        var order = await _payPalClient.CapturePayment(token);

        if (order.Status == OrderStatus.Completed)
        {
            var item = order.PurchaseUnits.FirstOrDefault()?.Items.FirstOrDefault();

            if (item?.Name == "Charge")
            {
                var chargeRepository = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CurrentlyChargingRepository>();
                var charge = await chargeRepository.GetByIdAsync(id);

                charge.ToPay = false;

                await chargeRepository.UpdateAsync(charge);
            }
            else if (item?.Name == "Stopover")
            {
                var stopoverRepository = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<StopoverRepository>();
                var stopover = await stopoverRepository.GetByIdAsync(id);

                stopover.ToPay = false;

                await stopoverRepository.UpdateAsync(stopover);
            }
            else
            {
                return BadRequest();
            }
        }

        return RedirectToPage("/payments");
    }

    [AllowAnonymous]
    [HttpGet("cancel")]
    public async Task<ActionResult> Cancel(string token, string payerId)
    {
        return Ok();
    }

    [HttpGet("payments")]
    public async Task<ActionResult> GetPayments([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] bool? chargeType)
    {
        if (startDate == default || endDate == default)
            return BadRequest();

        var payments = await _paymentHistoryRepository.GetPaymentsWithinDateRangeAndType(startDate, endDate, chargeType);
        return Ok(payments);
    }
}
