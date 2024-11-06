using Microsoft.AspNetCore.Mvc;

namespace YumYum.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult MyRecipeEdit()
        {
            return View();
        }

        public IActionResult MyRecipeCollect()
        {
            return View();
        }
    }
}
