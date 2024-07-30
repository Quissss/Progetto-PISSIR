using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Pages
{
    public class ReservationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ParkingRepository _parkingRepository;
        private readonly ILogger<ReservationModel> _logger;
        private readonly CarRepository _carRepository;

        public ReservationModel(UserManager<IdentityUser> userManager, ParkingRepository parkingRepository, ILogger<ReservationModel> logger, CarRepository carRepository)
        {
            _userManager = userManager;
            _parkingRepository = parkingRepository;
            _logger = logger;
            _carRepository = carRepository;
        }

        [BindProperty]
        public Reservation Reservation { get; set; } = new();
        public List<Parking> Parkings { get; set; } = new();

        public IEnumerable<Car> Cars { get; set; }

        public async Task OnGetAsync()
        {


            var currentUser = await _userManager.GetUserAsync(User);

            Reservation.UserId = currentUser.Id;
            Reservation.User = currentUser;
            Reservation.ReservationTime = DateTime.Now;

            Parkings = await _parkingRepository.GetAllAsync();
            Cars = await _carRepository.GetCarsByOwner(currentUser.Id);

        }
    }
}