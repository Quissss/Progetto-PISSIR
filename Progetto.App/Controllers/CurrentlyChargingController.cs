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
        private readonly CurrentlyChargingRepository _currentlyCharging;
        private readonly ILogger<CurrentlyChargingController> _logger;

        public CurrentlyChargingController(
            ILogger<CurrentlyChargingController> logger,
            UserManager<IdentityUser> userManager,
            CurrentlyChargingRepository currentlyCharging)
        {
            _logger = logger;
            _userManager = userManager;
            _currentlyCharging = currentlyCharging;
        }

        [HttpGet]
        public async Task<IActionResult> GetRecharges()
        {
            var currentUser = await _userManager.GetUserAsync(User);
           

            var recharges = await _currentlyCharging.GetByUserId(currentUser.Id);
            return Ok(recharges);
        }
    }
}