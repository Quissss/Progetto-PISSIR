using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Progetto.App.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{

    [AllowAnonymous]
    [HttpGet("/api/success")]
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
    [HttpGet("/api/failed")]
    public async Task<ActionResult> Fail(string id, string token, string? payerId, string returnUrl)
    {
        if (id is null || token is null)
            return BadRequest();
        //Failed
        return Ok();
    }
}
