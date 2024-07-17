using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Pages
{
    public class CarModel : PageModel
    {
        private readonly CarRepository _carRepository;
        private readonly ILogger<CarModel> _logger;

        public CarModel(CarRepository carRepository, ILogger<CarModel> logger)
        {
            _carRepository = carRepository;
            _logger = logger;
        }

        public IEnumerable<Car> Cars { get; private set; }

        public async Task OnGet()
        {
            _logger.LogDebug("Requested page {path}", Request.Path);

            Cars = await _carRepository.GetAllAsync();

        }
    }
}
