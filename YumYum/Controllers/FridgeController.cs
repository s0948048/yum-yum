using Microsoft.AspNetCore.Mvc;
using YumYum.Models;

namespace YumYum.Controllers
{
    public class FridgeController : Controller
    {
        private readonly YumYumDbContext _context;
        public IActionResult Index()
        {
            var viewModel = _context.RefrigeratorStores.ToList();
            return View(viewModel);
        }

        public IActionResult Edit()
        {
            return View();
        }
        public FridgeController(YumYumDbContext context)
        {
            _context = context;
        }
    }
}
