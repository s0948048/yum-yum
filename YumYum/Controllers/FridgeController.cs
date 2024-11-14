using Microsoft.AspNetCore.Mvc;
using YumYum.Models;
using YumYum.Models.ViewModels;

namespace YumYum.Controllers
{
    public class FridgeController : Controller
    {
        private readonly YumYumDbContext _context;

        public IActionResult Index()
        {
            var fridgeItemData = (from fridge in _context.RefrigeratorStores
                             join igd in _context.Ingredients on fridge.IngredientId equals igd.IngredientId
                             join unit in _context.Units on fridge.UnitId equals unit.UnitId
                             where fridge.UserId == 3204 
                             orderby fridge.ValidDate
                             select new FridgeItemViewModel
                             {
                                 UserID = fridge.UserId,
                                 IngredientName = igd.IngredientName,
                                 IngredientIcon = igd.IngredientIcon,
                                 Quantity = fridge.Quantity,
                                 UnitName = unit.UnitName,
                                 ValidDate = fridge.ValidDate
                             }
                             ).ToList();

            var ingredientData = (from igd in _context.Ingredients
                                  select new IngredientViewModel
                                  {
                                      IngredientName = igd.IngredientName,
                                      IngredientIcon = igd.IngredientIcon
                                  }).ToList();

            var viewModel = new FridgeViewModel
            {
                RefrigeratorData = fridgeItemData,
                IngredientData = ingredientData
            };

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
