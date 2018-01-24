using System.ComponentModel.DataAnnotations;

namespace OdeToFood.Models
{
    public class Restaurant
    {
        public int Id { get; set; }
        
        [Display(Name="Restaurant Name")]
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        
        [Display(Name="Cuisine")]
        public CuisineType CuisineType { get; set; }
    }
}