using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
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
                        Body = $"感謝您註冊yumyum會員，以下是您的驗證碼「{varifycode}」",
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
    }
}
