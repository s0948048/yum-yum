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
            return View();
        }

        public IActionResult MyRecipeCollect()
        {
            return View();
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
