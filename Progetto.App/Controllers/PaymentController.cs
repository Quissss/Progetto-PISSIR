using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using PayPal.REST.Client;
using PayPal.REST.Models;
using PayPal.REST.Models.Orders;
using PayPal.REST.Models.PaymentSources;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly PaymentHistoryRepository _paymentHistoryRepository;
    private readonly PayPalClient _payPalClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PaymentController(PaymentHistoryRepository paymentHistoryRepository, IServiceScopeFactory serviceScopeFactory)//, PayPalClient payPalClient)
    {
        _paymentHistoryRepository = paymentHistoryRepository;
        _serviceScopeFactory = serviceScopeFactory;
        //_payPalClient = payPalClient;
    }

    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequest orderRequest)
    {
        try
        {
            var orderResponse = await _payPalClient.CreateOrder(orderRequest);
            return Ok(orderResponse);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("confirm-order")]
    public async Task<IActionResult> ConfirmOrder(string orderId, [FromBody] IPaymentSource paymentSource)
    {
        try
        {
            var confirmedOrder = await _payPalClient.ConfirmOrder(orderId, paymentSource);
            return Ok(confirmedOrder);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("capture-payment")]
    public async Task<IActionResult> CapturePayment(string orderId)
    {
        try
        {
            var capturedPayment = await _payPalClient.CapturePayment(orderId);
            return Ok(capturedPayment);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
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
