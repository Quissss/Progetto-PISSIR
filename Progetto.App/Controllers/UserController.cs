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

    /// <summary>
    /// Controller per la gestione degli utenti (operazioni CRUD).
    /// Richiede autenticazione.
    /// </summary>
    /// <param name="userManager">Gestore delle operazioni sugli utenti</param>
    public UserController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Ottiene i dettagli dell'utente attualmente autenticato.
    /// </summary>
    /// <returns>Ritorna l'ID dell'utente autenticato se trovato, altrimenti un errore 404.</returns>
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

    /// <summary>
    /// Aggiorna l'utente attuale al ruolo Premium.
    /// Implementazione di un sistema di pagamento con PayPal da completare.
    /// </summary>
    /// <returns>Ritorna un messaggio di successo se l'utente è stato aggiornato a Premium, altrimenti un errore.</returns>
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