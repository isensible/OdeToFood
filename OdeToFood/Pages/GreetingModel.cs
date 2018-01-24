using Microsoft.AspNetCore.Mvc.RazorPages;
using OdeToFood.Services;

namespace OdeToFood.Pages
{
    public class GreetingModel : PageModel
    {
        private readonly IGreeter _greeter;

        public GreetingModel(IGreeter greeter)
        {
            _greeter = greeter;
        }

        public string CurrentGreeting { get; set; }
        
        /// <summary>
        /// Invoked on HTTP GET
        /// </summary>
        public void OnGet(string name)
        {
            CurrentGreeting = $"{name} says: {_greeter.GetMessageOfTheDay()}";
        }
    }
}