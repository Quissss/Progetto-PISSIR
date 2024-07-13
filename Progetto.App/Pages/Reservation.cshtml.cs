using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Progetto.App.Core.Models;
using System;
using System.Threading.Tasks;

namespace Progetto.App.Pages
{
    public class ReservationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ReservationModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public Reservation Reservation { get; set; } = new ();

        public async Task OnGetAsync()
        {
         

            var currentUser = await _userManager.GetUserAsync(User);

            Reservation.UserId = currentUser.Id;
            Reservation.User = currentUser;
            Reservation.ReservationTime = DateTime.Now;


        }
    }
}