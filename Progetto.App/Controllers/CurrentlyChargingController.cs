using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CurrentlyChargingController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly CurrentlyChargingRepository _currentlyChargingRespository;
    private readonly StopoverRepository _stopoverRepository;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<CurrentlyChargingController> _logger;

    public CurrentlyChargingController(
        ILogger<CurrentlyChargingController> logger,
        UserManager<IdentityUser> userManager,
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

    [HttpPost("historicizeCharge")]
    public async Task<IActionResult> HistoricizeCharge([FromBody] int chargeId)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var paymentHistoryRepository = scope.ServiceProvider.GetRequiredService<PaymentHistoryRepository>();

        var charge = await _currentlyChargingRespository.GetByIdAsync(chargeId);
        var historicizedRecharge = await paymentHistoryRepository.AddAsync(new PaymentHistory
        {
            StartTime = charge.StartChargingTime.Value,
            EndTime = charge.EndChargingTime.Value,
            StartChargePercentage = charge.StartChargePercentage,
            EndChargePercentage = charge.TargetChargePercentage,
            UserId = charge.UserId,
            CarPlate = charge.CarPlate,
            EnergyConsumed = charge.EnergyConsumed,
            TotalCost = charge.TotalCost,
            IsCharge = true,
        });
        await _currentlyChargingRespository.DeleteAsync(c => c.Id == chargeId);

        return Ok(historicizedRecharge);
    }

    [HttpPost("historicizeStopover")]
    public async Task<IActionResult> HistoricizeStopover([FromBody] int stopoverId)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var paymentHistoryRepository = scope.ServiceProvider.GetRequiredService<PaymentHistoryRepository>();

        var stopover = await _stopoverRepository.GetByIdAsync(stopoverId);
        var historicizedStopover = await paymentHistoryRepository.AddAsync(new PaymentHistory
        {
            StartTime = stopover.StartStopoverTime.Value,
            EndTime = stopover.EndStopoverTime.Value,
            UserId = stopover.UserId,
            CarPlate = stopover.CarPlate,
            TotalCost = stopover.TotalCost,
            IsCharge = false,
        });
        await _stopoverRepository.DeleteAsync(s => s.Id == stopoverId);

        return Ok(historicizedStopover);
    }
}