using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Data;
using Progetto.App.Core.Models.Users;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UserController> _logger;

    /// <summary>
    /// Controller per la gestione degli utenti (operazioni CRUD).
    /// Richiede autenticazione.
    /// </summary>
    /// <param name="userManager">Gestore delle operazioni sugli utenti</param>
    public UserController(SignInManager<ApplicationUser> signInManager, ApplicationDbContext dbContext, ILogger<UserController> logger)
    {
        _signInManager = signInManager;
        _userManager = signInManager.UserManager;
        _dbContext = dbContext;
        _logger = logger;
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
    [HttpPost("upgrade")]
    [AllowAnonymous]
    public async Task<IActionResult> UpgradeToPremium([FromQuery]string value)
    {
        _logger.LogDebug("Changing standard user to premium");
        if(value is null)
            return BadRequest("Malformed request");

        var user = _dbContext.Users.Where(x => x.Id == value).First();
        if (user is null)
            return BadRequest("Malformed request");

        var claim = _dbContext.UserClaims.Where(x => x.ClaimType == ClaimName.Role && x.UserId == user.Id).First();
        _logger.LogDebug("{claim}", claim);
        claim.ClaimValue = ((int)Role.PremiumUser).ToString();
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Utente aggiornato a Premium con successo!" });
    }
}