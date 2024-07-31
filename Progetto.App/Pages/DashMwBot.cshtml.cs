using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Progetto.App.Core.Models;
using Progetto.App.Core.Repositories;

namespace Progetto.App.Pages;

public class DashMwBotModel : PageModel
{
    public IEnumerable<SelectListItem> Parkings { get; set; }

    public async Task OnGet()
    {

    }
}