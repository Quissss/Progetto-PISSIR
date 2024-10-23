using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Progetto.App.Core.Models.Users;

namespace Progetto.App.Pages;

public class CarModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    public string UserId { get; private set; }

    public CarModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task OnGet()
    {
        UserId = (await _userManager.GetUserAsync(User))?.Id.ToString();
    }
}
