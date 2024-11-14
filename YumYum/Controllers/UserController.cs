using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YumYum.Models;

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
        public async Task<IActionResult> Index()
        {
            return View();
        }


        public IActionResult EditInfo()
		{
			return View();
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


            //sql db test--成功連上
            string? RecipeName = (from xa in _context.RecipeBriefs
                                  where xa.RecipeId == 1399
                                  select xa.RecipeName).SingleOrDefault();
            ViewBag.recipeName = RecipeName;

            return View();

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

            //return View();


            //sql db test2
            var data = from xa in _context.RecipeBriefs
                       where xa.RecipeId > 1399 && xa.RecipeId <= 1404

                       select new RecipeBrief
                       {
                           RecipeName = xa.RecipeName,
                           //RecipeShot = xa.RecipeShot
                       };

            return View(data.ToList());
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
                return Json(new { redirectUrl = Url.Action("Index", "Home") });
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
    }
}
