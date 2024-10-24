using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Models.Users;
using Progetto.App.Core.Repositories;
using Progetto.App.Core.Security;

namespace Progetto.App.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CurrentlyChargingController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CurrentlyChargingRepository _currentlyChargingRespository;
    private readonly StopoverRepository _stopoverRepository;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<CurrentlyChargingController> _logger;

    /// <summary>
    /// Controller per gestire le sessioni di ricarica attualmente in corso e le fermate.
    /// Richiede l'autenticazione.
    /// </summary>
    /// <param name="logger">Interfaccia per la gestione del logging.</param>
    /// <param name="userManager">Gestore utenti per operazioni di autenticazione e autorizzazione.</param>
    /// <param name="currentlyChargingRepository">Repository per gestire le informazioni sulle ricariche in corso.</param>
    /// <param name="stopoverRepository">Repository per gestire le fermate durante il viaggio.</param>
    /// <param name="serviceScopeFactory">Fornisce un contesto per eseguire servizi all'interno di uno scope.</param>
    public CurrentlyChargingController(
        ILogger<CurrentlyChargingController> logger,
        UserManager<ApplicationUser> userManager,
        CurrentlyChargingRepository currentlyChargingRepository,
        StopoverRepository stopoverRepository,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _userManager = userManager;
        _currentlyChargingRespository = currentlyChargingRepository;
        _stopoverRepository = stopoverRepository;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// Ottiene la lista delle sessioni di ricarica in corso per l'utente autenticato o per tutti gli utenti, se l'utente è un amministratore.
    /// </summary>
    /// <param name="carPlate">Opzionale: targa dell'auto per filtrare le sessioni di ricarica.</param>
    /// <returns>La lista delle sessioni di ricarica filtrata per targa, se fornita.</returns>
    [HttpGet("recharges")]
    public async Task<IActionResult> GetRecharges([FromQuery] string? carPlate)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var role = User?.Claims?.First(x => x.Type == ClaimName.Role).Value;
        var isAdmin = (int)Role.Admin == Convert.ToInt32(role);

        var recharges = isAdmin ? 
                await _currentlyChargingRespository.GetAllAsync() : 
                await _currentlyChargingRespository.GetByUserId(currentUser.Id);

        if (!(string.IsNullOrEmpty(carPlate)))
        {
            recharges = recharges.Where(r => r.CarPlate.Contains(carPlate, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        return Ok(recharges);
    }

    /// <summary>
    /// Ottiene la lista delle fermate per l'utente autenticato o per tutti gli utenti, se l'utente è un amministratore.
    /// </summary>
    /// <param name="carPlate">Opzionale: targa dell'auto per filtrare le fermate.</param>
    /// <returns>La lista delle fermate filtrata per targa, se fornita.</returns>
    [HttpGet("stopovers")]
    public async Task<IActionResult> GetStopovers([FromQuery] string? carPlate)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var role = User?.Claims?.First(x => x.Type == ClaimName.Role).Value;
        var isAdmin = (int)Role.Admin == Convert.ToInt32(role);

        var stopovers = isAdmin ?
                await _stopoverRepository.GetAllAsync() :
                await _stopoverRepository.GetByUserId(currentUser.Id);

        if (!(string.IsNullOrEmpty(carPlate)))
        {
            stopovers = stopovers.Where(s => s.CarPlate.Contains(carPlate, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        return Ok(stopovers);
    }
}