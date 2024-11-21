using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using YumYum.Models;
using YumYum.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YumYum.Controllers
{
    public class UserController : Controller
    {
        //引用資料庫
        private readonly YumYumDbContext _context;

        public UserController(YumYumDbContext context)
        {
            _context = context;
        }





		//健誠
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			int? userId = HttpContext.Session.GetInt32("userId");
			//int? userId = 3207;

			var userQuery = from user in _context.UserBios
							join userNickname in _context.UserSecretInfos
							on user.UserId equals userNickname.UserId
							where user.UserId == userId
							select new UserQueryViewModel
							{
								UserId = user.UserId,
								UserIntro = user.UserIntro,
								HeadShot = user.HeadShot,
								Igaccount = user.Igaccount,
								Fbnickname = user.Fbnickname,
								YoutuNickname = user.YoutuNickname,
								WebNickName = user.WebNickName,
								YoutuLink = user.YoutuLink,
								Fblink = user.Fblink,
								WebLink = user.WebLink,
								UserNickname = userNickname.UserNickname
							};

			var recipeQuery = from user in userQuery
							  join recipe in _context.RecipeBriefs
							  on user.UserId equals recipe.CreatorId
							  join recipeInfo in _context.RecipeRecords
							  on recipe.RecipeId equals recipeInfo.RecipeId
							  join recipeImage in _context.RecipeRecordFields
							  on recipe.RecipeId equals recipeImage.RecipeId
							  where recipeInfo.RecipeStatusCode == 1 || recipeInfo.RecipeStatusCode == 4
							  where recipeImage.RecipeField == 0
							  select new RecipeQueryViewModel
							  {
								  RecipeId = recipe.RecipeId,
								  RecipeName = recipe.RecipeName,
								  RecipeStatusCode = recipeInfo.RecipeStatusCode,
								  FieldShot = recipeImage.FieldShot,
							  };

			var recipeDetailQuery = from recipe in recipeQuery
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
				userQueryViewModel = userQuery.ToList(),
				recipeQueryViewModel = recipeQuery.ToList(),
				recipeDetailQuery = recipeDetailQuery.ToList(),
			};

			HttpContext.Session.SetInt32("userId", (int)userId);
			return View(AllList);


		}


		[HttpGet]
		public async Task<IActionResult> EditInfo()
		{
			int? userId = HttpContext.Session.GetInt32("userId");
			//int? userId = 3207;//for test
			UserSecretInfo? userSecretInfo = await _context.UserSecretInfos.Where(p => p.UserId == userId).FirstOrDefaultAsync();
			UserBio? userBio = await _context.UserBios.Where(p => p.UserId == userId).FirstOrDefaultAsync();
			if (userSecretInfo == null || userBio == null)
			{
				return NotFound();
			}
			var viewModel = new UserViewModel
			{
				UserId = userBio.UserId,
				HeadShot = userBio.HeadShot,
				UserIntro = userBio.UserIntro,
				Igaccount = userBio.Igaccount,
				Fbnickname = userBio.Fbnickname,
				YoutuNickname = userBio.YoutuNickname,
				WebNickName = userBio.WebNickName,
				YoutuLink = userBio.YoutuLink,
				Fblink = userBio.Fblink,
				WebLink = userBio.WebLink,
				UserNickname = userSecretInfo.UserNickname
			};
			HttpContext.Session.SetInt32("userId", (int)userId);
			return View(viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditInfo(UserViewModel viewModel)
		{
			int? userId = HttpContext.Session.GetInt32("userId");

			//int? userId = 3207;//for test					

			if (ModelState.IsValid)
			{
				UserSecretInfo? userSecretInfo = await _context.UserSecretInfos.Where(p => p.UserId == userId).FirstOrDefaultAsync();
				UserBio? userBio = await _context.UserBios.Where(p => p.UserId == userId).FirstOrDefaultAsync();

				if (userSecretInfo == null || userBio == null)
				{
					return NotFound();
				}

				userBio.UserIntro = viewModel.UserIntro;
				userBio.HeadShot = viewModel.HeadShot;
				userBio.Igaccount = viewModel.Igaccount;
				userBio.Fbnickname = viewModel.Fbnickname;
				userBio.YoutuNickname = viewModel.YoutuNickname;
				userBio.WebNickName = viewModel.WebNickName;
				userBio.YoutuLink = viewModel.YoutuLink;
				userBio.Fblink = viewModel.Fblink;
				userBio.WebLink = viewModel.WebLink;
				userSecretInfo.UserNickname = viewModel.UserNickname;

				_context.Update(userBio);
				_context.Update(userSecretInfo);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			HttpContext.Session.SetInt32("userId", (int)userId);
			return View(viewModel);

		}
		private bool UserBioExists(int id)
		{
			return _context.UserBios.Any(e => e.UserId == id);
		}

		//1120 Upload image		

		// POST: User/Upload
		[HttpPost]
		public async Task<IActionResult> Upload(IFormFile file)
		{
			//int? userId = HttpContext.Session.GetInt32("userId");
			int? userId = 3207;//for test		
			if (file != null && file.Length > 0)
			{
				var fileNamefromlocal = System.IO.Path.GetFileName(file.FileName);
				var sideFileName = fileNamefromlocal.Split('.').Last();
				var fileName = "HeadShot" + userId.ToString() + "." + sideFileName;
				var filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Img", fileName);
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}
				UserBio? userBio = await _context.UserBios.Where(p => p.UserId == userId).FirstOrDefaultAsync();
				userBio.HeadShot = "/Img/" + fileName;
				_context.Update(userBio);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(EditInfo));
			}
			return Json(new { success = false });


		}




		//芳慈
		public IActionResult MyRecipeEdit()
        {
            // 設定Breadcrumb 顯示頁面資訊
            ViewBag.Breadcrumbs = new List<BreadcrumbItem>
             {
             new BreadcrumbItem("首頁", Url.Action("Index", "Recipe") ?? "#"),
             new BreadcrumbItem("會員專區", Url.Action("Index", "User") ?? "#"),
             new BreadcrumbItem("我的食譜", "#") // 目前的頁面
             };


            //先設定的userId
            int userId = 3201;

            // 食譜基本資訊查詢
            var recipeData = from rb in _context.RecipeBriefs
                             join us in _context.UserSecretInfos on rb.CreatorId equals us.UserId
                             join rf in _context.RecipeRecordFields on rb.RecipeId equals rf.RecipeId
                             join rr in _context.RecipeRecords on rb.RecipeId equals rr.RecipeId
                             join ri in _context.RecipeIngredients on rb.RecipeId equals ri.RecipeId
                             join ig in _context.Ingredients on ri.IngredientId equals ig.IngredientId
                             where rf.RecipeRecVersion == (from r in _context.RecipeRecordFields
                                                           where r.RecipeId == rb.RecipeId
                                                           select r.RecipeRecVersion).Max()
                             && rf.RecipeField == 0
                             && us.UserId == userId  // Filter by UserID = 3201
                             select new MyRecipeViewModel.RecipeDetail
                             {
                                 RecipeID = (int)rb.RecipeId,
                                 RecipeName = rb.RecipeName,
                                 UserNickname = us.UserNickname,
                                 FinishMinute = rb.FinishMinute,
                                 FieldShot = rf.FieldShot,
                                 FieldDescript = rf.FieldDescript,
                                 RecipeStatusCode = rr.RecipeStatusCode,
                                 IngredientName = ig.IngredientName,
                                 RecipeRecVersion = rf.RecipeRecVersion
                             };



            // 合併data
            var viewModel = new MyRecipeViewModel
            {
                RecipeDetails = recipeData.ToList()
            };

            return View(viewModel);


        }

        public IActionResult MyRecipeCollect()
        {

            // 設定Breadcrumb 顯示頁面資訊
            ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
            new BreadcrumbItem("首頁", Url.Action("Index", "Recipe") ?? "#"),
            new BreadcrumbItem("會員專區", Url.Action("Index", "User") ?? "#"),
            new BreadcrumbItem("我的食譜", Url.Action("MyRecipeEdit", "User") ?? "#"),
            new BreadcrumbItem("收藏食譜", "#") // 目前的頁面
            };

            //先設定的userId
            int userId = 3201;

            // 食譜基本資訊查詢
            var recipeData = from rb in _context.RecipeBriefs
                             join uc in _context.UserCollectRecipes on rb.RecipeId equals uc.RecipeID
                             join us in _context.UserSecretInfos on rb.CreatorId equals us.UserId
                             join rf in _context.RecipeRecordFields on rb.RecipeId equals rf.RecipeId
                             join rr in _context.RecipeRecords on rb.RecipeId equals rr.RecipeId
                             join ri in _context.RecipeIngredients on rb.RecipeId equals ri.RecipeId
                             join ig in _context.Ingredients on ri.IngredientId equals ig.IngredientId
                             where rf.RecipeRecVersion == (from r in _context.RecipeRecordFields
                                                           where r.RecipeId == rb.RecipeId
                                                           select r.RecipeRecVersion).Max()
                             && rf.RecipeField == 0
                             && uc.UserID == userId  // Filter by UserID = 3201
                             select new MyRecipeViewModel.RecipeDetail
                             {
                                 RecipeID = (int)rb.RecipeId,
                                 RecipeName = rb.RecipeName,
                                 UserNickname = us.UserNickname,
                                 FinishMinute = rb.FinishMinute,
                                 FieldShot = rf.FieldShot,
                                 FieldDescript = rf.FieldDescript,
                                 RecipeStatusCode = rr.RecipeStatusCode,
                                 IngredientName = ig.IngredientName

                             };


            // 合併data
            var viewModel = new MyRecipeViewModel
            {
                RecipeDetails = recipeData.ToList()
            };

            return View(viewModel);
        }


        [Route("User/Upload")]
        [HttpPost]
        public async Task<IActionResult> Upload(int recipeId, int recipeRecVersion)
        {
            Console.WriteLine($"Updating RecipeID: {recipeId}, RecipeRecVersion: {recipeRecVersion}");
            try
            {
                var recipeUpload = await _context.RecipeRecords.FirstOrDefaultAsync(
                    r => r.RecipeId == recipeId && r.RecipeRecVersion == recipeRecVersion);

                if (recipeUpload == null)
                {
                    Console.WriteLine("Record not found.");
                    return Json(new { success = false, message = "食譜記錄未找到。" });
                }

                recipeUpload.RecipeStatusCode = 1;
                await _context.SaveChangesAsync();
                Console.WriteLine("Update successful.");

                return Json(new { success = true, message = "成功發布食譜！" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "發生錯誤，請稍後再試。" });
            }
        }








        //毅祥
        public async Task<IActionResult> LogInPage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogInPage([FromBody] UserSecretInfo user)
        {
            UserSecretInfo users = await _context.UserSecretInfos.Where(p => p.Email == user.Email).FirstOrDefaultAsync();

            if (users != null)
            {
                HttpContext.Session.SetInt32("userId", users.UserId);
                return Json(new { redirectUrl = Url.Action("Index", "Recipe") });
            }
            else
            {
                return Json(new { errorMessage = "帳號未註冊" });
            }
        }

        public async Task<IActionResult> RegisterPage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterPage([FromBody] UserSecretInfo user)
        {
            UserSecretInfo users = await _context.UserSecretInfos.Where(p => p.Email == user.Email).FirstOrDefaultAsync();

            if (users != null)
            {
                return Json(new { registerdMessage = "信箱已有人使用，請更換信箱" });
            }
            else
            {
                //驗證碼
                var random = new Random();
                string varifycode = "";
                for (int i = 0; i < 6; i++)
                {
                    varifycode += random.Next(0, 10);
                }
                //將資料傳遞給驗證頁面
                //將驗證碼資料新增進user裡
                user.EmailValidCode = varifycode;
                HttpContext.Session.SetString("registerInformation", System.Text.Json.JsonSerializer.Serialize(user));
                //寄送驗證信
                try
                {
                    //SMTP設置
                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("yumyumService@gmail.com", "keiqmcldmxuiqmon"),
                        EnableSsl = true,
                    };
                    // 設定郵件內容
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("yumyumService@gmail.com"),
                        Subject = "YumYum註冊會員驗證信",
                        Body = @$"
                           <div style=""width: 500px; height: 200px;border: 2px solid black;text-align: center;background-color: #fffae6;border-radius:10px"">
                           <p
                           style="" margin-top:88px;font-family:'Lucida Sans', 'Lucida Sans Regular', 'Lucida Grande', 'Lucida Sans Unicode', Geneva, Verdana, sans-serif;font-size:16px"">
                           感謝您註冊YumYum信箱，以下是您的驗證碼<b>「{varifycode}」</b></p>
                           </div>",
                        IsBodyHtml = true, // 設定為 HTML 格式
                    };
                    mailMessage.To.Add(user.Email);
                    //發送郵件
                    await smtpClient.SendMailAsync(mailMessage);
                    return Json(new { action = Url.Action("RegisterVerifyPage", "user"), mailsuccess = "寄送信件成功" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"郵件發送失敗: {ex.Message}", action = Url.Action("RegisterVerifyPage", "user") });
                }

            }
        }
        public async Task<IActionResult> RegisterVerifyPage()
        {
            //註冊者資訊
            string registerInfor = HttpContext.Session.GetString("registerInformation")!;
            if (!string.IsNullOrEmpty(registerInfor))
            {
                var JsonData = JsonConvert.DeserializeObject<UserSecretInfo>(registerInfor);
                return View("RegisterVerifyPage", JsonData);
            }
            else
            {
                return View();
            }

        }
        [HttpPost]
        public async Task<IActionResult> RegisterVerifyPage([FromBody] UserSecretInfo user)
        {
            await _context.UserSecretInfos.AddAsync(user);
            await _context.SaveChangesAsync();
            return Json(new { action = Url.Action("Index", "Recipe"), successmessage = "成功新增會員資料" });
        }


        [HttpPost]
        public async Task<IActionResult> SendVerifyAgain([FromBody] string Email)
        {
            //驗證碼
            var random = new Random();
            string varifycode = "";
            for (int i = 0; i < 6; i++)
            {
                varifycode += random.Next(0, 10);
            }
            string registerInfor = HttpContext.Session.GetString("registerInformation")!;
            if (!string.IsNullOrEmpty(registerInfor))
            {
                var registerInfroChange = System.Text.Json.JsonSerializer.Deserialize<UserSecretInfo>(registerInfor);
                registerInfroChange.EmailValidCode = varifycode;
                HttpContext.Session.SetString("registerInformation", System.Text.Json.JsonSerializer.Serialize(registerInfroChange));
            }
            //寄送驗證信
            try
            {
                //SMTP設置
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("yumyumService@gmail.com", "keiqmcldmxuiqmon"),
                    EnableSsl = true,
                };
                // 設定郵件內容
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("yumyumService@gmail.com"),
                    Subject = "YumYum註冊會員驗證信",
                    Body = $"感謝您註冊yumyum會員，以下是您的驗證碼「{varifycode}」",
                    IsBodyHtml = true, // 設定為 HTML 格式
                };
                mailMessage.To.Add(Email);
                //發送郵件
                await smtpClient.SendMailAsync(mailMessage);
                return Json(new { sucessmessage = "重新寄送成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"郵件發送失敗: {ex.Message}", action = Url.Action("RegisterVerifyPage", "user") });
            }
        }



        public async Task<IActionResult> ChangePasswordPage()
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            UserSecretInfo user = await _context.UserSecretInfos.FirstOrDefaultAsync(u => u.UserId == userId);
            if (userId != null)
            {
                return View(user);
            }
            else
            {
                return View();
            }

        }
        [HttpPut]
        public async Task<IActionResult> ChangePasswordPage([FromBody] UserSecretInfo user)
        {
            UserSecretInfo changePasswordUser = await _context.UserSecretInfos.FirstOrDefaultAsync(p => p.Email == user.Email);
            changePasswordUser!.Password = user.Password;
            var saveResult = await _context.SaveChangesAsync();
            if (saveResult != 0)
            {
                return Json(new { message = "變更密碼成功" });
            }
            else { return Json(new { message = "資料存取發生錯誤" }); }
        }
    }
}
