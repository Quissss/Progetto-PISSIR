using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Pages
{
    public class ServicesModel : PageModel
    {
        private readonly CarRepository _carRepository;
        private readonly ILogger<ServicesModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        public IEnumerable<Car> Cars { get; private set; }
        public string UserId { get; private set; }

        public ServicesModel(CarRepository carRepository, ILogger<ServicesModel> logger, UserManager<IdentityUser> userManager)
        {
            _carRepository = carRepository;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task OnGet()
        {
            UserId = (await _userManager.GetUserAsync(User))?.Id;

            var cars = (await _carRepository.GetCarsByOwner(UserId)) ?? [];
            Cars = cars.Where(c => c.Status == CarStatus.Waiting).ToList();
        }
    }
}
