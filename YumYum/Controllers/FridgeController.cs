using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
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
            var ingredientData = GetIngredientData(userId: null);
            var fridgeItemData = GetFridgeItemData();
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
        public IActionResult UpdateRefrigeratorStore(List<FridgeItemViewModel> RefrigeratorItems, List<FridgeItemViewModel> NewRefrigeratorItems)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(new { seccess = false, message = "錯誤資料格式。" });
            }

            IQueryable<int> Newstoreid = (IQueryable<int>)RefrigeratorItems
                    .Where(o => o.StoreID.HasValue) // 過濾掉 null
                    .Select(o => o.StoreID!.Value)   // 取出值
                    .AsQueryable();

            IQueryable<int> ExsistStoreid = _context.RefrigeratorStores.Where(r => r.UserId == 3204).Select(r => r.StoreId);

            // 舊的比新的還多出的 => 要刪除的
            var delItems = ExsistStoreid.Except(Newstoreid).ToList();

            // 要修改ㄉ
            var UpdateItems = ExsistStoreid.Where(r => !delItems.Contains(r)).ToList();

            // Update existing items
            foreach (var item in RefrigeratorItems.Where(r => UpdateItems.Contains((int)r.StoreID!)))
            {
                var record = _context.RefrigeratorStores.FirstOrDefault(r => r.StoreId == item.StoreID && r.UserId == 3204);
                if (record != null)
                {
                    record.UnitId = Convert.ToInt16(item.UnitName);
                    record.Quantity = item.Quantity!;
                    record.ValidDate = item.ValidDate;
                }
            }

            // Delete No items
            foreach (var item in RefrigeratorItems.Where(r => delItems.Contains((int)r.StoreID!)))
            {
                var record = _context.RefrigeratorStores.FirstOrDefault(r => r.StoreId == item.StoreID && r.UserId == 3204);
                _context.RefrigeratorStores.Remove(record!);
            }

            // Add new items    1. 舊有食材、新的食材
            foreach (var newItem in NewRefrigeratorItems)
            {
                if (newItem.NewIngredientCreate is null)
                {
                    var newRecord = new RefrigeratorStore
                    {
                        UserId = 3204,
                        IngredientId = newItem.IngredientID,
                        Quantity = newItem.Quantity!,
                        UnitId = Convert.ToInt16(newItem.UnitName),
                        ValidDate = newItem.ValidDate
                    };
                    _context.RefrigeratorStores.Add(newRecord);
                }
                else if (newItem.NewIngredientCreate.Length > 0)
                {
                    var newIg = _context.Ingredients.Add(new Ingredient
                    {
                        IngredientName = newItem.NewIngredientCreate,
                        AttributionId = 9
                    });
                    _context.RefrigeratorStores.Add(new RefrigeratorStore
                    {
                        UserId = 3204,
                        Ingredient = newIg.Entity,
                        Quantity = newItem.Quantity!,
                        UnitId = Convert.ToInt16(newItem.UnitName),
                        ValidDate = newItem.ValidDate
                    });
                }
                _context.SaveChanges();
            }

            var fridgeItemData = GetFridgeItemData();
            var ingredientData = GetIngredientData();

            var viewModel = new FridgeViewModel
            {
                RefrigeratorData = fridgeItemData,
                IngredientData = ingredientData
            };
            return View("Index", viewModel);// Redirect back to the main view after saving changes
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
                .Select(i => new
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
                    join unit in _context.Units on fridge.UnitId equals unit.UnitId
                    where fridge.UserId == 3204
                    orderby fridge.ValidDate
                    select new FridgeItemViewModel
                    {
                        StoreID = fridge.StoreId,
                        UserID = fridge.UserId,
                        IngredientID = fridge.IngredientId,
                        IngredientName = fridge.Ingredient.IngredientName,
                        IngredientIcon = fridge.Ingredient.IngredientIcon,
                        Quantity = fridge.Quantity,
                        UnitName = unit.UnitName,
                        ValidDate = fridge.ValidDate,
                        IngredAttributeUnit = new SelectList(_context.Units.Where(u => u.IngredAttributeId == fridge.Ingredient.AttributionId).ToList(), "UnitId", "UnitName", fridge.UnitId)

                    }).ToList();
        }

        // Get left Ingredient/tag button list, filter if needed
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
                IngredientID = igd.IngredientId,
                IngredientName = igd.IngredientName,
                IngredientIcon = igd.IngredientIcon,
            }).ToList();
        }

        // Get offcanvas IngredAttributes for filtering
        private List<IngredAttributeViewModel> GetIngredAttributeData()
        {
            return (from ia in _context.IngredAttributes
                    orderby ia.IngredAttributeId
                    select new IngredAttributeViewModel
                    {
                        IngredAttributeID = ia.IngredAttributeId,
                        IngredAttributeName = ia.IngredAttributeName,
                        IngredAttributePhoto = ia.IngredAttributePhoto,
                    }
                ).ToList();
        }
    }
}
