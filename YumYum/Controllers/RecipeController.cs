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
            //此區與此區的view更動中
            //接收食譜的id
            int? recipeId = HttpContext.Session.GetInt32("recipeId");
            //測試(之後會刪掉)->
            int recipeIdTest = 1402;
            //得到資料
            //取得食譜內容
            var recipeBrief = from recipe in await _context.RecipeBriefs.Where(p => p.RecipeId == recipeIdTest).ToListAsync()
                              join className in await _context.RecipeClasses.ToListAsync()
                              on recipe.RecipeClassId equals className.RecipeClassId
                              select new RecipeWatch_Brief
                              {
                                  //食譜名稱
                                  recipeName = recipe.RecipeName,
                                  //完成時間
                                  recipeFinishTime = recipe.FinishMinute,
                                  //食用人數
                                  recipePeople = recipe.PersonQuantity,
                                  //食譜類別
                                  className = className.RecipeClassName,
                              };
            //取得最新版本號
            var recipeStepAndIntroVersion = await _context.RecipeRecordFields.Where(p => p.RecipeId == recipeIdTest).MaxAsync(x => x.RecipeRecVersion);
            //取得最新版本的內容(簡介、食譜圖片、步驟圖片內容)
            var recipeStepAndIntro = await _context.RecipeRecordFields.Where(p => p.RecipeId == recipeIdTest && p.RecipeRecVersion == recipeStepAndIntroVersion).ToListAsync();
            //取得食譜食材
            var recipeIngredient = from data in await _context.RecipeIngredients.ToListAsync()
                                   join ingre in await _context.Ingredients.ToListAsync()
                                   on data.IngredientId equals ingre.IngredientId
                                   join unit in await _context.Units.ToListAsync()
                                   on data.UnitId equals unit.UnitId
                                   where data.RecipeId == recipeIdTest
                                   select new RecipeWatch_Ingredient
                                   {
                                       //食材名稱
                                       ingredientName = ingre.IngredientName,
                                       //食材icon
                                       ingredientImg = ingre.IngredientIcon,
                                       //食材數量
                                       ingredientCount = data.Quantity,
                                       //食材單位
                                       ingredientunit = unit.UnitName,
                                   };

            var AllList = new RecipeWatch()
            {
                recipeWatchBrief = recipeBrief.ToList(),
                recipeRecordFields = recipeStepAndIntro.ToList(),
                recipeIngredient = recipeIngredient.ToList(),
            };
            return View(AllList);
        }

        public async Task<IActionResult> CreateRecipe()
        {
            var ingredientList = await _context.Ingredients.ToListAsync();
            var classList = await _context.RecipeClasses.ToListAsync();
            var unitList = await _context.Units.ToListAsync();

            var allList = new RecipeCreate()
            {
                Ingredients = ingredientList,
                className = classList,
                units = unitList
            };


            //取得食材
            return View(allList);

        }
        //編輯食譜 js css還暫套跟createRecipe一樣的之後差別是還要導入查看食譜的資料
        public async Task<IActionResult> EditRecipe()
        {
            var ingredientList = await _context.Ingredients.ToListAsync();
            var classList = await _context.RecipeClasses.ToListAsync();
            var unitList = await _context.Units.ToListAsync();

            var allList = new RecipeCreate()
            {
                Ingredients = ingredientList,
                className = classList,
                units = unitList
            };


            //取得食材
            return View(allList);

        }


        //健誠
    }
}
