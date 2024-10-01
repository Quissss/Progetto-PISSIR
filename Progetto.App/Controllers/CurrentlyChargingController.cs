using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Models.Users;
using Progetto.App.Core.Repositories;

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

    [HttpGet("recharges")]
    public async Task<IActionResult> GetRecharges([FromQuery] string? carPlate)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var recharges = await _currentlyChargingRespository.GetByUserId(currentUser.Id);

        if (!(string.IsNullOrEmpty(carPlate)))
        {
            recharges = recharges.Where(r => r.CarPlate.Contains(carPlate, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        return Ok(recharges);
    }

    [HttpGet("stopovers")]
    public async Task<IActionResult> GetStopovers([FromQuery] string? carPlate)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var stopovers = await _stopoverRepository.GetByUserId(currentUser.Id);

        if (!(string.IsNullOrEmpty(carPlate)))
        {
            stopovers = stopovers.Where(s => s.CarPlate.Contains(carPlate, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        return Ok(stopovers);
    }
}