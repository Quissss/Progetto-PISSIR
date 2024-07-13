using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Progetto.App.Pages
{
    public class AirpodsModel : PageModel
    {
        private readonly ILogger<AirpodsModel> _logger;

        public AirpodsModel(ILogger<AirpodsModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}
