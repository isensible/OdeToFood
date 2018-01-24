using Microsoft.Extensions.Configuration;

namespace OdeToFood.Services
{
    public interface IGreeter
    {
        string GetMessageOfTheDay();
    }

    public class Greeter : IGreeter
    {
        private readonly string _greeting;
        
        public Greeter(IConfiguration configuration)
        {
            _greeting = configuration["Greeting"];
        }
        
        public string GetMessageOfTheDay()
        {
            return _greeting;
        }
    }
}