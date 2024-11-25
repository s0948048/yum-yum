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
            var ingredientData = GetIngredientData(userId: null);
            var ingredAttributeData = GetIngredAttributeData();

            var viewModel = new FridgeViewModel
            {
                RefrigeratorData = fridgeItemData,
                IngredientData = ingredientData,
                IngredAttributeData = ingredAttributeData
            };
            
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult UpdateRefrigeratorStore(List<FridgeItemViewModel> RefrigeratorItems)
        {
            if (ModelState.IsValid)
            {
                foreach (var item in RefrigeratorItems)
                {
                    var record = _context.RefrigeratorStores.FirstOrDefault(r => r.StoreId == item.StoreID && r.UserId == 3204);
                    if (record != null)
                    {
                        record.Quantity = item.Quantity!;
                        record.ValidDate = item.ValidDate;
                    }
                }

                _context.SaveChanges();

                return RedirectToAction("Index"); // Redirect back to the main view after saving changes
            }
            return View();
        }


        [HttpGet]
        public IActionResult GetOtherUnits()
        {
            var otherUnits = _context.Units
                .Where(u => u.IngredAttributeId == 9)
                .OrderBy(u => u.UnitId)
                .Select(u => new
                {
                    UnitId = u.UnitId,
                    UnitName = u.UnitName
                }).ToList();

            return Json(otherUnits);
        }

        [HttpGet]
        public IActionResult GetUnitsByIngredientName(string ingredientName)
        {
            var unitsList = (
                from u in _context.Units
                join i in _context.Ingredients on u.IngredAttributeId equals i.AttributionId
                where i.IngredientName == ingredientName
                orderby i.AttributionId
                select new
                {
                    UnitId = u.UnitId,
                    UnitName = u.UnitName
                }
                ).ToList();

            return Json(unitsList);
        }

        [HttpPost]
        public JsonResult FilterIngredients([FromBody] List<int> selectedIds)
        {
            var filteredIngredients = _context.Ingredients.AsQueryable();
            if (selectedIds != null && selectedIds.Any()) 
            {
                filteredIngredients = filteredIngredients.Where(i => selectedIds.Contains(i.AttributionId));
            }

            var result = filteredIngredients
                .Select(i  => new
                {
                    IngredientName = i.IngredientName,
                    IngredientIcon = Url.Content($"~{i.IngredientIcon}")
                }).ToList();

            return Json(result);
        }

        public IActionResult SearchIngredients(string searchKeyword)
        {
            var allIngredients = GetIngredientData();

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                allIngredients = allIngredients
                    .Where(i => i.IngredientName!.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return PartialView("_IngredientListPartial", allIngredients);
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
                        StoreID = fridge.StoreId,
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

        private List<IngredAttributeViewModel> GetIngredAttributeData()
        {
            return (from ia in _context.IngredAttributes
                    orderby ia.IngredAttributeId
                    select new IngredAttributeViewModel
                    {
                        IngredAttributeID = ia.IngredAttributeId,
                        IngredAttributeName = ia.IngredAttributeName,
                        IngredAttributePhoto = ia.IngredAttributePhoto
                    }
                ).ToList(); 
        }
    }
}
