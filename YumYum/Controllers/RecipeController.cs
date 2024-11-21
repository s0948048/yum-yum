﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Text.Json;
using YumYum.Models;
using YumYum.Models.Recipe;
using YumYum.Models.ViewModels;

namespace YumYum.Controllers
{
    public class RecipeController : Controller
    {
        //導入資料庫內容
        private readonly YumYumDbContext _context;
        //用來取得根目錄
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RecipeController(YumYumDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        //Index

        //健誠
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            //int? userId = 3204;

            // 查詢所有食材
            var ingredientQuery = from ingredient in await _context.Ingredients.ToListAsync()
                                  select new
                                  {
                                      IngredientName = ingredient.IngredientName,
                                      IngredientId = ingredient.IngredientId,
                                      IngredientIcon = ingredient.IngredientIcon,
                                  };
            if (userId != null)
            {
                // 查詢使用者擁有的食材
                var userIngredientQuery = from user in await _context.RefrigeratorStores.ToListAsync()
                                          join userIngredient in ingredientQuery on user.IngredientId equals userIngredient.IngredientId
                                          where user.UserId == userId
                                          select new
                                          {
                                              UserId = user.UserId,
                                              IngredientName = userIngredient.IngredientName,
                                              IngredientId = userIngredient.IngredientId,
                                              IngredientIcon = userIngredient.IngredientIcon
                                          };
                return View(userIngredientQuery);
            }
            else
            {
                return View(ingredientQuery);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] List<string> selectedValues)
        {
            //int? userId = HttpContext.Session.GetInt32("userId");		

            var ingredientQuery = from attr in await _context.IngredAttributes.ToListAsync()
                                  join ingredient in await _context.Ingredients.ToListAsync()
                                  on attr.IngredAttributeId equals ingredient.AttributionId
                                  where selectedValues.Contains(ingredient.AttributionId.ToString())
                                  select new
                                  {
                                      IngredientName = ingredient.IngredientName,
                                      IngredientId = ingredient.IngredientId,
                                      IngredientIcon = ingredient.IngredientIcon
                                  };

            return PartialView("_PartialTags", ingredientQuery);
        }

        [HttpPost]
        public async Task<IActionResult> IndexQuery([FromBody] List<string> selectedValues)
        {
            foreach (var item in selectedValues)
            {
                Console.WriteLine(item);
            }
            var recipeQuery = from recipe in _context.RecipeBriefs
                              join recipeInfo in _context.RecipeRecords
                              on recipe.RecipeId equals recipeInfo.RecipeId
                              join recipeImage in _context.RecipeRecordFields
                              on recipe.RecipeId equals recipeImage.RecipeId
                              join recipeIngredient in _context.RecipeIngredients
                              on recipe.RecipeId equals recipeIngredient.RecipeId
                              where (recipeInfo.RecipeStatusCode == 1 || recipeInfo.RecipeStatusCode == 4)
                              && (recipeImage.RecipeField == 0)
                              && selectedValues.Contains(recipeIngredient.IngredientId.ToString())
                              select new RecipeQueryViewModel
                              {
                                  RecipeId = recipe.RecipeId,
                                  RecipeName = recipe.RecipeName,
                                  FieldShot = recipeImage.FieldShot,
                                  FinishMinute = recipe.FinishMinute
                              };

            var recipeDetailQuery = from recipe in recipeQuery.Distinct()
                                    join recipeIngredient in _context.RecipeIngredients on recipe.RecipeId equals recipeIngredient.RecipeId
                                    join recipeIngredientName in _context.Ingredients on recipeIngredient.IngredientId equals recipeIngredientName.IngredientId
                                    select new RecipeDetailQuery
                                    {
                                        RecipeId = recipeIngredient.RecipeId,
                                        IngredientId = recipeIngredient.IngredientId,
                                        IngredientName = recipeIngredientName.IngredientName
                                    };
            var AllList = new RecipeAllUser()
            {
                recipeQueryViewModel = recipeQuery.Distinct().ToList(),
                recipeDetailQuery = recipeDetailQuery.ToList()
            };
            return PartialView("_PartialRecipe", AllList);

        }

        [HttpPost]
        public async Task<IActionResult> IngredientQuery([FromBody] SearchRequest request)
        {

            string searchString = request.SearchString;
            if (searchString != String.Empty)
            {
                // 查詢食材
                var ingredientQuery = from ingredient in await _context.Ingredients.ToListAsync()
                                      where ingredient.IngredientName.Contains(searchString)
                                      select new
                                      {
                                          IngredientName = ingredient.IngredientName,
                                          IngredientId = ingredient.IngredientId,
                                          IngredientIcon = ingredient.IngredientIcon,
                                      };
                return PartialView("_PartialTags", ingredientQuery);
            }
            else
            {
                // 查詢食材
                var ingredientQueryAll = from ingredient in await _context.Ingredients.ToListAsync()
                                         select new
                                         {
                                             IngredientName = ingredient.IngredientName,
                                             IngredientId = ingredient.IngredientId,
                                             IngredientIcon = ingredient.IngredientIcon,
                                         };
                return PartialView("_PartialTags", ingredientQueryAll);
            }

        }
        public class SearchRequest { public string SearchString { get; set; } }


        public async Task<IActionResult> WatchRecipe()
        {
            //此區與此區的view更動中
            //接收食譜的id
            int? recipeId = HttpContext.Session.GetInt32("recipeId");
            int? userId = HttpContext.Session.GetInt32("userId") == null ? 0 : HttpContext.Session.GetInt32("userId");
            HttpContext.Session.SetInt32("userId", (int)userId!);
            //測試(之後會刪掉)->
            int recipeIdTest = 1412;
            //得到資料
            //取得食譜內容
            var recipeBrief = from recipe in await _context.RecipeBriefs.Where(p => p.RecipeId == recipeIdTest).ToListAsync()
                              join className in await _context.RecipeClasses.ToListAsync()
                              on recipe.RecipeClassId equals className.RecipeClassId
                              join user in await _context.UserSecretInfos.ToListAsync()
                              on recipe.CreatorId equals user.UserId
                              join userPhoto in await _context.UserBios.ToListAsync()
                              on user.UserId equals userPhoto.UserId
                              select new RecipeWatch_Brief
                              {
                                  //建立食譜人得名稱
                                  userNickName = user.UserNickname,
                                  //建立食譜的人的頭像
                                  userPhoto = userPhoto.HeadShot,
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
            var recipeStepAndIntro = await _context.RecipeRecordFields.Where(p => p.RecipeId == recipeIdTest && p.RecipeRecVersion == recipeStepAndIntroVersion).OrderBy(p => p.RecipeField).ToListAsync();
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
                userId = userId,
                recipeId = recipeIdTest,
            };
            return View(AllList);
        }

        //收藏食譜
        [HttpPost]
        public async Task<IActionResult> WatchRecipe([FromBody] ReipeWatch_Collect uc)
        {
            try
            {
                var collect = await _context.UserCollectRecipes.FirstOrDefaultAsync(p => p.UserID == uc.UserID && p.RecipeID == (short)uc.RecipeID);


                if (uc.verifyColor == "transparent")
                {
                    if (collect != null)
                    {
                        _context.UserCollectRecipes.Remove(collect);

                    }
                }
                else if (uc.verifyColor == "#30533f")
                {
                    if (collect == null)
                    {
                        UserCollectRecipe data = new UserCollectRecipe()
                        {
                            RecipeID = (short)uc.RecipeID,
                            UserID = uc.UserID,
                        };
                        _context.UserCollectRecipes.Add(data);

                    }
                }
                await _context.SaveChangesAsync();
                return Json(new { success = "成功", message = collect });
            }
            catch (Exception ex)
            {

                return Json(new { success = uc.verifyColor, message = ex.InnerException != null ? ex.InnerException.Message : ex.Message });
            }
        }
        public async Task<IActionResult> CreateRecipe()
        {
            var ingredientList = await _context.Ingredients.ToListAsync();
            var classList = await _context.RecipeClasses.ToListAsync();
            var unitList = await _context.Units.ToListAsync();
            var attribute = from attri in await _context.IngredAttributes.ToListAsync()
                            select new RecipeCreate_attribute
                            {
                                IngredAttributeId = attri.IngredAttributeId,
                                IngredAttributeName = attri.IngredAttributeName,
                                IngredAttributePhoto = attri.IngredAttributePhoto,
                            };
            //userId刷新
            int? userId = HttpContext.Session.GetInt32("userId") == null ? 0 : HttpContext.Session.GetInt32("userId");
            HttpContext.Session.SetInt32("userId", (int)userId!);
            var allList = new RecipeCreate()
            {
                Ingredients = ingredientList,
                className = classList,
                units = unitList,
                attributes = attribute.ToList(),
                userId = userId,
            };


            //取得食材
            return View(allList);

        }


        //接收創建食譜的資訊

        [HttpPost]
        public async Task<IActionResult> CreateRecipe([FromBody] RecipeCreate_Save saveData)
        {
            if (saveData == null)
            {
                return Json(new { success = false, message = "我是null" });
            }

            saveData.recipeBrief.Creator = await _context.UserSecretInfos.FindAsync(saveData.recipeBrief.CreatorId);
            saveData.recipeBrief.RecipeClass = await _context.RecipeClasses.FindAsync(saveData.recipeBrief.RecipeClassId);

            if (saveData.recipeBrief.Creator == null || saveData.recipeBrief.RecipeClass == null)
            {
                return Json(new { success = false, message = "creator null" });
            }

            // 清理 ModelState

            if (!TryValidateModel(saveData))
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                return Json(new { success = false, message = errors });
            }


            _context.RecipeBriefs.Add(saveData.recipeBrief);
            await _context.SaveChangesAsync();
            //將食譜的id提出出來

            var recipe = _context.RecipeBriefs.FirstOrDefault(p => p.RecipeName == saveData.recipeBrief.RecipeName && saveData.recipeBrief.CreateDate.Date == p.CreateDate.Date);
            string recipeID = recipe!.RecipeId.ToString();
            short recipeIDshort = recipe!.RecipeId;
            //如果是自建食材，就新增食材進入資料庫
            for (int i = 0; i < saveData.recipeIngredients.Count; i++)
            {
                saveData.recipeIngredients[i].RecipeId = recipeIDshort;
                if (saveData.recipeIngredients[i].IngredientId == 0)
                {
                    Ingredient ingredient = new Ingredient()
                    {
                        IngredientName = saveData.ingredientNames[i],
                        AttributionId = 9,
                        IngredientIcon = "/img/icon/EmptyTag.svg"
                    };
                    _context.Ingredients.Add(ingredient);
                    await _context.SaveChangesAsync();
                    Ingredient data = await _context.Ingredients.FirstOrDefaultAsync(p => p.IngredientName == saveData.ingredientNames[i] && p.AttributionId == 9);
                    saveData.recipeIngredients[i].IngredientId = data.IngredientId;
                }
            }
            //存取紀錄資料與食材資料
            _context.RecipeIngredients.AddRange(saveData.recipeIngredients);
            saveData.recipeRecord.RecipeId = recipeIDshort;
            _context.RecipeRecords.Add(saveData.recipeRecord);
            await _context.SaveChangesAsync();
            //先將圖片存入img folder並將saveData的shot資料都改成相對路徑的格式
            if (saveData.recipeRecordFields != null && saveData.recipeRecordFields.Count > 0)
            {
                //將圖片的資料取出來並去除前綴詞
                //因為Icollection無法使用索引器方法，先將資料轉換成list
                var RecipeRecordFieldsList = saveData.recipeRecordFields.ToList();
                bool hasUnknownField = false;
                for (int i = 0; i < RecipeRecordFieldsList.Count; i++)
                {
                    //設定前端未傳回的資料recipeid(後端食譜存進去才會有)
                    RecipeRecordFieldsList[i].RecipeId = recipeIDshort;
                    RecipeRecordFieldsList[i].RecipeFieldNavigation = await _context.RecipeFields.FindAsync(RecipeRecordFieldsList[i].RecipeField);
                    string mimetype = string.Empty;
                    //如果是預設圖片就不要存入資料夾
                    if (RecipeRecordFieldsList[i].FieldShot == "/img/icon/AddPhoto.png")
                    {
                        RecipeRecordFieldsList[i].FieldShot = "/img/icon/AddPhoto.png";
                        _context.RecipeRecordFields.Add(RecipeRecordFieldsList[i]);
                    }
                    else if (RecipeRecordFieldsList[i].FieldShot!.StartsWith("data:image/jpeg;base64,"))
                    {
                        if (i == 0)
                        {
                            mimetype = "image/jpeg";
                            var fieldShot = RecipeRecordFieldsList[i].FieldShot!.Replace("data:image/jpeg;base64,", "");
                            byte[] imageBytes = Convert.FromBase64String(fieldShot);
                            //設定副檔名及檔名
                            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "img/recipe/recipe_intro");
                            string fileName = recipeID + '_' + i + ".jpg";
                            string fullfilename = Path.Combine(folderPath, fileName);
                            //將圖片存進資料夾
                            System.IO.File.WriteAllBytes(fullfilename, imageBytes);
                            //將saveData的資料存成相對路徑
                            RecipeRecordFieldsList[i].FieldShot = $"/img/recipe/recipe_intro/{fileName}";
                            //將步驟資料存進資料庫
                            _context.RecipeRecordFields.Add(RecipeRecordFieldsList[i]);
                        }
                        else
                        {
                            mimetype = "image/jpeg";
                            var fieldShot = RecipeRecordFieldsList[i].FieldShot!.Replace("data:image/jpeg;base64,", "");
                            byte[] imageBytes = Convert.FromBase64String(fieldShot);
                            //設定副檔名及檔名
                            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "img/recipe/recipe_step");
                            string fileName = recipeID + '_' + i + ".jpg";
                            string fullfilename = Path.Combine(folderPath, fileName);
                            //將圖片存進資料夾
                            System.IO.File.WriteAllBytes(fullfilename, imageBytes);
                            //將saveData的資料存成相對路徑
                            RecipeRecordFieldsList[i].FieldShot = $"/img/recipe/recipe_step/{fileName}";
                            //將步驟資料存進資料庫
                            _context.RecipeRecordFields.Add(RecipeRecordFieldsList[i]);
                        }
                    }
                    else if (RecipeRecordFieldsList[i].FieldShot!.StartsWith("data:image/png;base64,"))
                    {
                        if (i == 0)
                        {
                            mimetype = "image/png";
                            var fieldShot = RecipeRecordFieldsList[i].FieldShot!.Replace("data:image/png;base64,", "");
                            byte[] imageBytes = Convert.FromBase64String(fieldShot);
                            //設定副檔名及檔名
                            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "img/recipe/recipe_intro");
                            string fileName = recipeID + '_' + i + ".png";
                            string fullfilename = Path.Combine(folderPath, fileName);
                            //將圖片存進資料夾
                            System.IO.File.WriteAllBytes(fullfilename, imageBytes);
                            //將saveData的資料存成相對路徑
                            RecipeRecordFieldsList[i].FieldShot = $"/img/recipe/recipe_intro/{fileName}";
                            //將步驟資料存進資料庫
                            _context.RecipeRecordFields.Add(RecipeRecordFieldsList[i]);
                        }
                        else
                        {
                            mimetype = "image/png";
                            var fieldShot = RecipeRecordFieldsList[i].FieldShot!.Replace("data:image/png;base64,", "");
                            byte[] imageBytes = Convert.FromBase64String(fieldShot);
                            //設定副檔名及檔名
                            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "img/recipe/recipe_step");
                            string fileName = recipeID + '_' + i + ".png";
                            string fullfilename = Path.Combine(folderPath, fileName);
                            //將圖片存進資料夾
                            System.IO.File.WriteAllBytes(fullfilename, imageBytes);
                            //將saveData的資料存成相對路徑
                            RecipeRecordFieldsList[i].FieldShot = $"/img/recipe/recipe_step/{fileName}";
                            //將步驟資料存進資料庫
                            _context.RecipeRecordFields.Add(RecipeRecordFieldsList[i]);
                        }

                    }
                    else
                    {
                        hasUnknownField = true;
                    }
                    if (hasUnknownField)
                    {
                        return Json(new { success = "出現未知錯誤", fieldShot = RecipeRecordFieldsList[i].FieldShot });
                    }
                }
            }
            //存取紀錄、步驟、食材資料
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return Json(new { success = false, message = "資料儲存失敗", error = ex.InnerException?.Message ?? ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "系統錯誤", error = ex.Message });
            }

            //回傳新建食譜成功
            return Json(new { success = "新建食譜成功" });
        }


        //編輯食譜
        public async Task<IActionResult> EditRecipe()
        {
            //以下是拿食譜資訊
            //此區與此區的view更動中
            //接收食譜的id
            int? recipeId = HttpContext.Session.GetInt32("recipeId");
            //測試(之後會刪掉)->
            int recipeIdTest = 1565;
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
            //取得最新版本號之食譜狀態碼
            var recipeNewestRecord = await _context.RecipeRecords.FirstOrDefaultAsync(p => p.RecipeId == recipeIdTest && p.RecipeRecVersion == recipeStepAndIntroVersion);
            var recipeNewestRecordStatus = recipeNewestRecord.RecipeStatusCode;
            //取得最新版本的內容(簡介、食譜圖片、步驟圖片內容)
            var recipeStepAndIntro = await _context.RecipeRecordFields.Where(p => p.RecipeId == recipeIdTest && p.RecipeRecVersion == recipeStepAndIntroVersion).OrderBy(p => p.RecipeField).ToListAsync();
            //取得食譜食材
            var recipeIngredient = from data in await _context.RecipeIngredients.ToListAsync()
                                   where data.RecipeId == recipeIdTest
                                   select new RecipeEdit_Ingredient
                                   {
                                       //食材id
                                       ingredientId = data.Ingredient.IngredientId,
                                       //食材類型
                                       ingredientAttribute = data.Ingredient.AttributionId,
                                       //食材名稱
                                       ingredientName = data.Ingredient.IngredientName,
                                       //食材icon
                                       ingredientImg = data.Ingredient.IngredientIcon,
                                       //食材數量
                                       ingredientCount = data.Quantity,
                                       //食材單位Id
                                       unitId = data.Unit.UnitId,
                                       //食材單位
                                       ingredientunit = data.Unit.UnitName,
                                   };

            //以下是拿食材、類別、單位資訊
            var ingredientList = await _context.Ingredients.ToListAsync();
            var classList = await _context.RecipeClasses.ToListAsync();
            var unitList = await _context.Units.ToListAsync();
            var attribute = from attri in await _context.IngredAttributes.ToListAsync()
                            select new RecipeCreate_attribute
                            {
                                IngredAttributeId = attri.IngredAttributeId,
                                IngredAttributeName = attri.IngredAttributeName,
                                IngredAttributePhoto = attri.IngredAttributePhoto,
                            };
            //userId刷新
            int? userId = HttpContext.Session.GetInt32("userId") == null ? 0 : HttpContext.Session.GetInt32("userId");
            HttpContext.Session.SetInt32("userId", (int)userId!);
            var allList = new RecipeEdit_Get()
            {
                //資料庫所有關於食材、食譜類型、單位、食材屬性的資料
                Ingredients = ingredientList,
                className = classList,
                units = unitList,
                attributes = attribute.ToList(),
                //要修改的食譜資料
                userId = userId,
                recipeWatchBrief = recipeBrief.ToList(),
                recipeRecordFields = recipeStepAndIntro.ToList(),
                recipeIngredient = recipeIngredient.ToList(),
                //版本號及狀態碼
                reipeVersion = recipeStepAndIntroVersion,
                recipeStatusCode = recipeNewestRecordStatus,

            };


            //取得食材
            return View(allList);

        }


        //接收編輯食譜的資訊

        [HttpPost]
        public async Task<IActionResult> EditRecipe([FromBody] RecipeCreate_Save saveData)
        {
            if (saveData == null)
            {
                return Json(new { success = false, message = "我是null" });
            }

            saveData.recipeBrief.Creator = await _context.UserSecretInfos.FindAsync(saveData.recipeBrief.CreatorId);
            saveData.recipeBrief.RecipeClass = await _context.RecipeClasses.FindAsync(saveData.recipeBrief.RecipeClassId);

            if (saveData.recipeBrief.Creator == null || saveData.recipeBrief.RecipeClass == null)
            {
                return Json(new { success = false, message = "creator null" });
            }

            // 清理 ModelState

            if (!TryValidateModel(saveData))
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                return Json(new { success = false, message = errors });
            }


            _context.RecipeBriefs.Update(saveData.recipeBrief);
            await _context.SaveChangesAsync();
            //將食譜的id提出出來
            var recipe = _context.RecipeBriefs.FirstOrDefault(p => p.RecipeName == saveData.recipeBrief.RecipeName && saveData.recipeBrief.CreateDate.Date == p.CreateDate.Date);
            string recipeID = recipe!.RecipeId.ToString();
            short recipeIDshort = recipe!.RecipeId;
            //如果是自建食材，就新增食材進入資料庫
            for (int i = 0; i < saveData.recipeIngredients.Count; i++)
            {
                saveData.recipeIngredients[i].RecipeId = recipeIDshort;
                if (saveData.recipeIngredients[i].IngredientId == 0)
                {
                    Ingredient ingredient = new Ingredient()
                    {
                        IngredientName = saveData.ingredientNames[i],
                        AttributionId = 9,
                        IngredientIcon = "/img/icon/EmptyTag.svg"
                    };
                    _context.Ingredients.Add(ingredient);
                    await _context.SaveChangesAsync();
                    Ingredient data = await _context.Ingredients.FirstOrDefaultAsync(p => p.IngredientName == saveData.ingredientNames[i] && p.AttributionId == 9);
                    saveData.recipeIngredients[i].IngredientId = data.IngredientId;
                }
            }
            //存取紀錄資料與食材資料

            var myIngredientDatas = await _context.RecipeIngredients.Where(p => p.RecipeId == recipeIDshort).ToDictionaryAsync(p => p.IngredientId);
            foreach (var data in saveData.recipeIngredients)
            {

                if (myIngredientDatas.TryGetValue(data.IngredientId, out var myIngredientData))
                {
                    myIngredientData.Quantity = data.Quantity;
                    myIngredientData.UnitId = data.UnitId;

                }
                else
                {
                    _context.RecipeIngredients.Add(data);
                }
            }
            saveData.recipeRecord!.RecipeId = recipeIDshort;
            _context.RecipeRecords.Add(saveData.recipeRecord);
            await _context.SaveChangesAsync();

            if (saveData.recipeRecordFields != null && saveData.recipeRecordFields.Count > 0)
            {
                //將圖片的資料取出來並去除前綴詞
                //因為Icollection無法使用索引器方法，先將資料轉換成list
                var RecipeRecordFieldsList = saveData.recipeRecordFields.OrderBy(p => p.RecipeField).ToList();
                bool hasUnknownField = false;
                for (int i = 0; i < RecipeRecordFieldsList.Count; i++)
                {
                    //設定前端未傳回的資料recipeid(後端食譜存進去才會有)
                    RecipeRecordFieldsList[i].RecipeId = recipeIDshort;
                    RecipeRecordFieldsList[i].RecipeFieldNavigation = await _context.RecipeFields.FindAsync(RecipeRecordFieldsList[i].RecipeField);
                    string mimetype = string.Empty;
                    //如果是預設圖片就不要存入資料夾
                    if (RecipeRecordFieldsList[i].FieldShot == "/img/icon/AddPhoto.png")
                    {
                        RecipeRecordFieldsList[i].FieldShot = "/img/icon/AddPhoto.png";
                        _context.RecipeRecordFields.Add(RecipeRecordFieldsList[i]);
                    }
                    else if (RecipeRecordFieldsList[i].FieldShot.StartsWith("/img"))
                    {
                        RecipeRecordFieldsList[i].FieldShot = RecipeRecordFieldsList[i].FieldShot;
                        _context.RecipeRecordFields.Add(RecipeRecordFieldsList[i]);
                    }
                    else if (RecipeRecordFieldsList[i].FieldShot!.StartsWith("data:image/jpeg;base64,"))
                    {
                        if (i == 0)
                        {
                            mimetype = "image/jpeg";
                            var fieldShot = RecipeRecordFieldsList[i].FieldShot!.Replace("data:image/jpeg;base64,", "");
                            byte[] imageBytes = Convert.FromBase64String(fieldShot);
                            //設定副檔名及檔名
                            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "img/recipe/recipe_intro");
                            string fileName = recipeID + '_' + i + ".jpg";
                            string fullfilename = Path.Combine(folderPath, fileName);
                            //將圖片存進資料夾
                            System.IO.File.WriteAllBytes(fullfilename, imageBytes);
                            //將saveData的資料存成相對路徑
                            RecipeRecordFieldsList[i].FieldShot = $"/img/recipe/recipe_intro/{fileName}";
                            //將步驟資料存進資料庫
                            _context.RecipeRecordFields.Add(RecipeRecordFieldsList[i]);
                        }
                        else
                        {
                            mimetype = "image/jpeg";
                            var fieldShot = RecipeRecordFieldsList[i].FieldShot!.Replace("data:image/jpeg;base64,", "");
                            byte[] imageBytes = Convert.FromBase64String(fieldShot);
                            //設定副檔名及檔名
                            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "img/recipe/recipe_step");
                            string fileName = recipeID + '_' + i + ".jpg";
                            string fullfilename = Path.Combine(folderPath, fileName);
                            //將圖片存進資料夾
                            System.IO.File.WriteAllBytes(fullfilename, imageBytes);
                            //將saveData的資料存成相對路徑
                            RecipeRecordFieldsList[i].FieldShot = $"/img/recipe/recipe_step/{fileName}";
                            //將步驟資料存進資料庫
                            _context.RecipeRecordFields.Add(RecipeRecordFieldsList[i]);
                        }
                    }
                    else if (RecipeRecordFieldsList[i].FieldShot!.StartsWith("data:image/png;base64,"))
                    {
                        if (i == 0)
                        {
                            mimetype = "image/png";
                            var fieldShot = RecipeRecordFieldsList[i].FieldShot!.Replace("data:image/png;base64,", "");
                            byte[] imageBytes = Convert.FromBase64String(fieldShot);
                            //設定副檔名及檔名
                            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "img/recipe/recipe_intro");
                            string fileName = recipeID + '_' + i + ".png";
                            string fullfilename = Path.Combine(folderPath, fileName);
                            //將圖片存進資料夾
                            System.IO.File.WriteAllBytes(fullfilename, imageBytes);
                            //將saveData的資料存成相對路徑
                            RecipeRecordFieldsList[i].FieldShot = $"/img/recipe/recipe_intro/{fileName}";
                            //將步驟資料存進資料庫
                            _context.RecipeRecordFields.Add(RecipeRecordFieldsList[i]);
                        }
                        else
                        {
                            mimetype = "image/png";
                            var fieldShot = RecipeRecordFieldsList[i].FieldShot!.Replace("data:image/png;base64,", "");
                            byte[] imageBytes = Convert.FromBase64String(fieldShot);
                            //設定副檔名及檔名
                            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "img/recipe/recipe_step");
                            string fileName = recipeID + '_' + i + ".png";
                            string fullfilename = Path.Combine(folderPath, fileName);
                            //將圖片存進資料夾
                            System.IO.File.WriteAllBytes(fullfilename, imageBytes);
                            //將saveData的資料存成相對路徑
                            RecipeRecordFieldsList[i].FieldShot = $"/img/recipe/recipe_step/{fileName}";
                            //將步驟資料存進資料庫
                            _context.RecipeRecordFields.Add(RecipeRecordFieldsList[i]);
                        }

                    }
                    else
                    {
                        hasUnknownField = true;
                    }
                    if (hasUnknownField)
                    {
                        return Json(new { success = "出現未知錯誤", fieldShot = RecipeRecordFieldsList[i].FieldShot });
                    }
                }
            }
            //存取紀錄、步驟、食材資料
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return Json(new { success = false, message = "資料儲存失敗", error = ex.InnerException?.Message ?? ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "系統錯誤", error = ex.Message });
            }


            //回傳新建食譜成功
            return Json(new { success = "編輯食譜成功" });
        }


        //健誠
    }
}
