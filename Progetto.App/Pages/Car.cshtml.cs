using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
