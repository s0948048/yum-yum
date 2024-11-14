using Microsoft.AspNetCore.Mvc;
using YumYum.Models;

namespace YumYum.Controllers
{
	public class CherishController : Controller
	{
		public IActionResult Introduce()
		{
			return View();
		}

		public IActionResult Manage()
		{
			return View();
		}

		public IActionResult Match()
		{
			return View();
		}
        
        

        public IActionResult ContactInformation()
		{
			// 設定 Breadcrumb
			ViewBag.Breadcrumbs = new List<BreadcrumbItem>{
			 new BreadcrumbItem("首頁", Url.Action("Index", "Recipe") ?? "#"),
			 new BreadcrumbItem("惜食專區", Url.Action("Introduce", "Cherish") ?? "#"),
			 new BreadcrumbItem("聯絡資料", "#") // 當前的頁面
             };

			return View();
		}

		//芳慈
        public IActionResult MatchHistory()
        {
            return View();
        }
        public IActionResult MatchHistoryOthers()
        {
            return View();
        }

        public IActionResult MatchHistoryMineInfo()
        {
            return View();
        }

        public IActionResult MatchHistoryOthersInfo()
        {
            return View();
        }
    }
}
