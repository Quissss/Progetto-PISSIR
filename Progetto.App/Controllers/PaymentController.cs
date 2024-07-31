using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly PaymentHistoryRepository _paymentHistoryRepository;

    public PaymentController(PaymentHistoryRepository paymentHistoryRepository)
    {
        _paymentHistoryRepository = paymentHistoryRepository;
    }

    [AllowAnonymous]
    [HttpGet("success")]
    public async Task<ActionResult> Success(string id, string token, string payerId, string returnUrl)
    {
        if (id is null || token is null)
            return BadRequest();
        var ok = Request.GetEncodedUrl();
        //var order = await _client.CapturePayment(token);
        // Renew policy
        return Ok();
    }

    [AllowAnonymous]
    [HttpGet("failed")]
    public async Task<ActionResult> Fail(string id, string token, string? payerId, string returnUrl)
    {
        if (id is null || token is null)
            return BadRequest();

        return Ok();
    }

    [Authorize]
    [HttpGet("payments")]
    public async Task<ActionResult> GetPayments([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] bool? chargeType)
    {
        if (startDate == default || endDate == default)
            return BadRequest();
        
        var payments = await _paymentHistoryRepository.GetPaymentsWithinDateRangeAndType(startDate, endDate, chargeType);
        return Ok(payments);
    }
}
