using Microsoft.AspNetCore.Mvc;
using OdeToFood.Models;
using OdeToFood.Services;
using OdeToFood.ViewModels;

namespace OdeToFood.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRestaurantData _restaurantData;
        private readonly IGreeter _greeter;

        public HomeController(IRestaurantData restaurantData, IGreeter greeter)
        {
            _restaurantData = restaurantData;
            _greeter = greeter;
        }
        
        public IActionResult Index()
        {
            var viewModel = new HomeIndexViewModel
            {
                Restaurants = _restaurantData.GetAll(),
                MessageOfTheDay = _greeter.GetMessageOfTheDay()
            };
            
            return View(viewModel);
        }
        
        public IActionResult Details(int id)
        {
            var model = _restaurantData.Get(id);

            if (model == null)
            {
                return RedirectToAction(nameof(Index));
            }
            
            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RestaurantEditModel editModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            
            var restaurant = _restaurantData.Add(new Restaurant
            {
                Name = editModel.Name,
                CuisineType = editModel.CuisineType
            });
            
            return RedirectToAction(nameof(Details), new { id = restaurant.Id });
        }
    }
}