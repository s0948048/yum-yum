using Microsoft.AspNetCore.Mvc;
using YumYum.Models;
using YumYum.Models.Customer;

namespace YumYum.Controllers
{
    public class FridgeController : Controller
    {
        private readonly YumYumDbContext _context;
        public IActionResult Index()
        {
            var viewModel = (from fridge in _context.RefrigeratorStores
                             join igd in _context.Ingredients on fridge.IngredientId equals         igd.IngredientId
                             join unit in _context.Units on fridge.UnitId equals unit.UnitId
                             where fridge.UserId == 3204 
                             orderby fridge.Quantity
                             select new RefrigeratorViewModel
                             {
                                 UserID = fridge.UserId,
                                 IngredientName = igd.IngredientName,
                                 Quantity = fridge.Quantity,
                                 UnitName = unit.UnitName,
                                 ValidDate = fridge.ValidDate
                             }
                             ).ToList();
            //var viewModel = _context.RefrigeratorStores.ToList();
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
