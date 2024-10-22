using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models.Users;
using System.Security.Claims;

namespace Progetto.App.Controllers;

/// <summary>
/// Controller for user operations (endpoints for CRUD operations)
/// Requires authentication
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(new { UserId = user.Id });
    }

    [HttpPost("upgrade-to-premium")]
    public async Task<IActionResult> UpgradeToPremium()
    {
        // TODO: Implement paypal payment
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Utente non trovato.");
        }

        var currentClaims = await _userManager.GetClaimsAsync(user);
        var premiumClaim = currentClaims.FirstOrDefault(c => c.Type == "IsPremium");

        if (premiumClaim == null)
        {
            await _userManager.AddClaimAsync(user, new Claim("IsPremium", "1"));
        }
        else
        {
            await _userManager.RemoveClaimAsync(user, premiumClaim);
            await _userManager.AddClaimAsync(user, new Claim("IsPremium", "1"));
        }

        return Ok(new { message = "Utente aggiornato a Premium con successo!" });
    }
}
