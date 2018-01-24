using Microsoft.AspNetCore.Mvc;

namespace OdeToFood.Controllers
{
    // Attribute route with [controller] token to take name from class
    // so this picks up "/about"
    [Route("[controller]")]
    public class AboutController
    {
        [Route("")]
        public string Phone()
        {
            return "0458 109311";
        }

        [Route("[action]")]
        public string Address()
        {
            return "2c Brentham Street, Leederville, WA 6007";
        }

        [Route("email")]
        public string Email()
        {
            return "ed.james@hotmail.co.uk";
        }

        [Route("company/[controller]/[action]")]
        public string Company()
        {
            return "isensible";
        }
    }
}