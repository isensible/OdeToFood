using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OdeToFood.Models;
using OdeToFood.Services;

namespace OdeToFood.Pages.Restaurants
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IRestaurantData _restaurantData;

        public EditModel(IRestaurantData restaurantData)
        {
            _restaurantData = restaurantData;
        }

        // Two way binding
        [BindProperty]
        public Restaurant Restaurant { get; set; }
        
        public IActionResult OnGet(int id)
        {
            Restaurant = _restaurantData.Get(id);

            if (Restaurant == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _restaurantData.Update(Restaurant);

            return RedirectToAction("Details", "Home", new {id = Restaurant.Id});
        }
    }
}