using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Progetto.App.Pages;

public class DashMwBotModel : PageModel
{
    public IEnumerable<SelectListItem> Parkings { get; set; }
}