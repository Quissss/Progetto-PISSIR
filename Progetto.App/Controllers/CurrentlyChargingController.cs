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
        private readonly StopoverHistoryRepository _stopoverHistoryRepository;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CurrentlyChargingController> _logger;

        public CurrentlyChargingController(
            ILogger<CurrentlyChargingController> logger,
            UserManager<IdentityUser> userManager,
            CurrentlyChargingRepository currentlyChargingRepository,
            StopoverHistoryRepository stopoverHistoryRepository,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _userManager = userManager;
            _currentlyChargingRespository = currentlyChargingRepository;
            _stopoverHistoryRepository = stopoverHistoryRepository;
            _serviceScopeFactory = serviceScopeFactory;
        }

        [HttpGet]
        public async Task<IActionResult> GetRecharges([FromQuery] string? carPlate)
        {
            var currentUser = await _userManager.GetUserAsync(User);
          
            var recharges = await _currentlyChargingRespository.GetByUserId(currentUser.Id);
              
            if (!(string.IsNullOrEmpty(carPlate)))
            {
                recharges = recharges.Where(r => r.CarPlate.Contains(carPlate,StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            return Ok(recharges);
        }

        [HttpPut("historicizeCharge")]
        public async Task<IActionResult> HistoricizeCharge([FromBody] CurrentlyCharging recharge)
        {
            var scope = _serviceScopeFactory.CreateScope();
            var chargeHistoryRepository = scope.ServiceProvider.GetRequiredService<ChargeHistoryRepository>();

            var historicizedRecharge = await chargeHistoryRepository.AddAsync(new ChargeHistory
            {
                StartChargingTime = recharge.StartChargingTime.Value,
                EndChargingTime = recharge.EndChargingTime.Value,
                StartChargePercentage = recharge.StartChargePercentage,
                TargetChargePercentage = recharge.TargetChargePercentage,
                MwBotId = recharge.MwBotId,
                UserId = recharge.UserId,
                CarPlate = recharge.CarPlate,
                ParkingSlotId = recharge.ParkingSlotId,
                EnergyConsumed = recharge.EnergyConsumed,
                TotalCost = recharge.TotalCost
            });

            return Ok(historicizedRecharge);
        }

        [HttpPut("historicizeStopover")]
        public async Task<IActionResult> HistoricizeStopover([FromBody] Stopover stopover)
        {
            var scope = _serviceScopeFactory.CreateScope();
            var stopoverHistoryRepository = scope.ServiceProvider.GetRequiredService<StopoverHistoryRepository>();

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
    }
}