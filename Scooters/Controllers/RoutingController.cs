using Microsoft.AspNetCore.Mvc;

namespace Scooters.Controllers
{
    public class RoutingController : Controller
    {
        public IActionResult Default()
        {
            return View();
        }
    }
}
