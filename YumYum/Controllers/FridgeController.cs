using Microsoft.AspNetCore.Mvc;
using YumYum.Models;
using YumYum.Models.ViewModels;

namespace YumYum.Controllers
{
    public class FridgeController : Controller
    {
        private readonly YumYumDbContext _context;

        public FridgeController(YumYumDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var fridgeItemData = GetFridgeItemData();
            var ingredientData = GetIngredientData();

            var viewModel = new FridgeViewModel
            {
                RefrigeratorData = fridgeItemData,
                IngredientData = ingredientData
            };
           
            return View(viewModel);
        }

        public IActionResult Edit()
        {
            var fridgeItemData = GetFridgeItemData();
            var ingredientData = GetIngredientData(userId: 3204);

            var viewModel = new FridgeViewModel
            {
                RefrigeratorData = fridgeItemData,
                IngredientData = ingredientData
            };
            
            return View(viewModel);
        }

        private List<FridgeItemViewModel> GetFridgeItemData()
        {
            return (from fridge in _context.RefrigeratorStores
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
                    }).ToList();
        }

        private List<IngredientViewModel> GetIngredientData(int? userId = null)
        {
            var ingredientsQuery = _context.Ingredients.AsQueryable();

            if (userId.HasValue) 
            {
                var existingIngredientIds = _context.RefrigeratorStores
                                             .Where(store => store.UserId == userId.Value)
                                             .Select(store => store.IngredientId);
                ingredientsQuery = ingredientsQuery.Where(igd => !existingIngredientIds.Contains(igd.IngredientId));
            }

            return ingredientsQuery.Select(igd => new IngredientViewModel
            {
                IngredientName = igd.IngredientName,
                IngredientIcon = igd.IngredientIcon
            }).ToList();
        }
    }
}
