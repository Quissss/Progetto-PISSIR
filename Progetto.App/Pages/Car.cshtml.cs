using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Pages
{
    public class CarModel : PageModel
    {
        private readonly CarRepository _carRepository;
        private readonly ILogger<CarModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;



        public CarModel(CarRepository carRepository, ILogger<CarModel> logger, UserManager<IdentityUser> userManager)
        {
            _carRepository = carRepository;
            _logger = logger;
            _userManager = userManager;

        }

        public IEnumerable<Car> Cars { get; private set; }
        public string UserId { get; private set; }

        public async Task OnGet()
        {
            _logger.LogDebug("Requested page {path}", Request.Path);

            Cars = await _carRepository.GetAllAsync();

            UserId = (await _userManager.GetUserAsync(User))?.Id.ToString();

        }



    }
}
