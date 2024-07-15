using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace Progetto.App.Pages
{
    public class ReservationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ParkingRepository _parkingRepository;

        public ReservationModel(UserManager<IdentityUser> userManager, ParkingRepository parkingRepository)
        {
            _userManager = userManager;
            _parkingRepository = parkingRepository;
        }

        [BindProperty]
        public Reservation Reservation { get; set; } = new ();
        public List<Parking> Parkings { get; set; } = new();

        public async Task OnGetAsync()
        {
         

            var currentUser = await _userManager.GetUserAsync(User);

            Reservation.UserId = currentUser.Id;
            Reservation.User = currentUser;
            Reservation.ReservationTime = DateTime.Now;

            Parkings = await _parkingRepository.GetAllAsync();

        }
    }
}