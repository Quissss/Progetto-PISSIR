using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Progetto.App.Controllers
{
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

        [HttpGet]
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

        [HttpPost("historicizeCharge")] // NOT WORKING
        public async Task<IActionResult> HistoricizeCharge([FromBody] int chargeId)
        {
            var scope = _serviceScopeFactory.CreateScope();
            var chargeHistoryRepository = scope.ServiceProvider.GetRequiredService<ChargeHistoryRepository>();

            var charge = await _currentlyChargingRespository.GetByIdAsync(chargeId);
            var historicizedRecharge = await chargeHistoryRepository.AddAsync(new ChargeHistory
            {
                StartChargingTime = charge.StartChargingTime.Value,
                EndChargingTime = charge.EndChargingTime.Value,
                StartChargePercentage = charge.StartChargePercentage,
                TargetChargePercentage = charge.TargetChargePercentage,
                MwBotId = charge.MwBotId,
                UserId = charge.UserId,
                CarPlate = charge.CarPlate,
                ParkingSlotId = charge.ParkingSlotId,
                EnergyConsumed = charge.EnergyConsumed,
                TotalCost = charge.TotalCost
            });

            return Ok(historicizedRecharge);
        }

        [HttpPost("historicizeStopover")] // NOT WORKING
        public async Task<IActionResult> HistoricizeStopover([FromBody] int stopoverId)
        {
            var scope = _serviceScopeFactory.CreateScope();
            var stopoverHistoryRepository = scope.ServiceProvider.GetRequiredService<StopoverHistoryRepository>();

            var stopover = await _stopoverRepository.GetByIdAsync(stopoverId);
            var historicizedStopover = await stopoverHistoryRepository.AddAsync(new StopoverHistory
            {
                StartStopoverTime = stopover.StartStopoverTime.Value,
                EndStopoverTime = stopover.EndStopoverTime.Value,
                UserId = stopover.UserId,
                CarPlate = stopover.CarPlate,
                TotalCost = stopover.TotalCost
            });

            return Ok(historicizedStopover);
        }

        [HttpPost("pay")]
        public async Task<IActionResult> Pay([FromForm] Stopover stopover)
        {
            var updateTopay = await _stopoverRepository.GetByIdAsync(stopover.Id);

            updateTopay.ToPay = false;
            await _stopoverRepository.SaveAsync();
            return Ok(updateTopay);
        }

        [HttpPost("payCharge")]
        public async Task<IActionResult> PayCharge([FromForm] CurrentlyCharging charge)
        {


            var updateTopay = await _currentlyChargingRespository.GetByIdAsync(charge.Id);

            updateTopay.ToPay = false;
            await _currentlyChargingRespository.SaveAsync();
            return Ok(updateTopay);
        }
    }
}