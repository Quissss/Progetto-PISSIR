using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Pages;

public class CarModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    public string UserId { get; private set; }

    public CarModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task OnGet()
    {
        UserId = (await _userManager.GetUserAsync(User))?.Id.ToString();
    }
}
