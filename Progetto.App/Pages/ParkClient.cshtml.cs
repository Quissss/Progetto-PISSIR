using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingLotApp.Pages
{
    public class ParkingLotModel : PageModel
    {
        public bool[] ParkingSpaces { get; set; } = new bool[10];

        public void OnGet()
        {
            // Simuliamo alcuni posti occupati
            ParkingSpaces[1] = true;
            ParkingSpaces[4] = true;
            ParkingSpaces[7] = true;
        }

        public void OnPostToggle(int index)
        {
            ParkingSpaces[index] = !ParkingSpaces[index];
        }
    }
}