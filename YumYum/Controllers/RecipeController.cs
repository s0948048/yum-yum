using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Text.Json;
using YumYum.Models;

namespace YumYum.Controllers
{
    public class RecipeController : Controller
    {
        //導入資料庫內容
        private readonly YumYumDbContext _context;

        public RecipeController(YumYumDbContext context)
        {
            _context = context;
        }

        //Index
        public async Task<IActionResult> Index()
        {
            return View();
        }


        //毅祥
        public async Task<IActionResult> WatchRecipe()
        {
            //接收食譜的id
            int? recipeId = HttpContext.Session.GetInt32("recipeId");
            //測試(之後會刪掉)->
            int recipeIdTest = 1402;
            //拿取食譜所需食材
            var ingredients = from ingreRecipe in await _context.RecipeIngredients.Where(id => id.RecipeId == recipeIdTest).ToListAsync()
                              join ingre in await _context.Ingredients.ToListAsync()
                              on ingreRecipe.IngredientId equals ingre.IngredientId
                              join ingreUnit in await _context.Units.ToListAsync()
                              on ingreRecipe.UnitId equals ingreUnit.UnitId
                              select new IngredientWatch
                              {
                                  //食材名稱
                                  ingredientName = ingre.IngredientName,
                                  //食材icon路徑
                                  ingredientImg = ingre.IngredientIcon,
                                  //食材數量
                                  ingredientCount = ingreRecipe.Quantity,
                                  //食材單位名稱
                                  ingredientsUnit = ingreUnit.UnitName,
                              };
            //因為session只能存字串或字串形式的物件 所以我們要將indredients序列化
            var ingredientsJson = JsonSerializer.Serialize(ingredients);
            //建立session
            TempData["ingredients"] = ingredientsJson;
            TempData.Keep("ingredients");

            //調用食譜內容
            var query = from data in await _context.RecipeBriefs.ToListAsync()
                        join step in await _context.RecipeSteps.ToListAsync()
                        on data.RecipeId equals step.RecipeId
                        join classname in await _context.RecipeClasses.ToListAsync()
                        on data.RecipeClassId equals classname.RecipeClassId
                        where data.RecipeId == recipeIdTest
                        select new RecipeWatch
                        {
                            //食譜名稱
                            recipeName = data.RecipeName,
                            //食譜照片
                            recipeImage = data.RecipeShot,
                            //類型
                            recipeClassName = classname.RecipeClassName,
                            //簡介
                            recipeDescription = data.RecipeDescript,
                            //時間
                            recipeFinishMinute = data.FinishMinute,
                            //人數
                            reciptPersonQuantity = data.PersonQuantity,
                            //步驟數
                            recipeStepNumber = step.StepNumber,
                            //步驟圖片
                            recipeStepsImage = step.StepShot,
                            //步驟敘述
                            recipeStepDescription = step.StepDescript,
                        };
            var datalist = query.ToList();
            return View(datalist);
        }

        public async Task<IActionResult> CreateRecipe()
        {
            //還在測試
            var ingredients = new List<string> { "雞蛋", "牛奶", "麵粉", "糖", "辣椒醬", "火龍果", "蘇打餅乾" };
            ViewData["Ingredients"] = ingredients;
            return View();

        }
        //編輯食譜 js css還暫套跟createRecipe一樣的
        public async Task<IActionResult> EditRecipe()
        {
            //還在測試
            var ingredients = new List<string> { "雞蛋", "牛奶", "麵粉", "糖", "辣椒醬", "火龍果", "蘇打餅乾" };
            ViewData["Ingredients"] = ingredients;
            return View();

        }


        //健誠
    }
}
