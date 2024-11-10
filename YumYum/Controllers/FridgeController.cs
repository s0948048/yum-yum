using Microsoft.AspNetCore.Mvc;

namespace YumYum.Controllers
{
    public class FridgeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
