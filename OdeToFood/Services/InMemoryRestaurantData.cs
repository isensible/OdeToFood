using System.Collections.Generic;
using System.Linq;
using OdeToFood.Models;

namespace OdeToFood.Services
{
    public class InMemoryRestaurantData : IRestaurantData
    {
        private readonly List<Restaurant> _restaurants;

        public InMemoryRestaurantData()
        {
            _restaurants = new List<Restaurant>
            {
                new Restaurant { Id=1, Name="Cindy's: Fast food, slow cooked"},
                new Restaurant { Id=2, Name="Blacks"},
                new Restaurant { Id=3, Name="Ed's Burgers" }
            };
        }
    
        public IEnumerable<Restaurant> GetAll()
        {
            return _restaurants.OrderBy(r => r.Name);
        }

        public Restaurant Get(int id)
        {
            return _restaurants.FirstOrDefault(r => r.Id == id);
        }

        public Restaurant Add(Restaurant restaurant)
        {
            var newRestaurant = new Restaurant
            {
                Id = GetNextId(),
                Name = restaurant.Name,
                CuisineType = restaurant.CuisineType
            };
            _restaurants.Add(newRestaurant);
            return newRestaurant;
        }

        public Restaurant Update(Restaurant restaurant)
        {
            // todo: implement!
            return restaurant;
        }

        private int GetNextId()
        {
            return _restaurants.Max(r => r.Id) + 1;
        }
    }
}