using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using YumYum.Models;
using YumYum.Models.DataTransferObject;

namespace YumYum.Controllers
{
	public class CherishController : Controller
	{
		private readonly YumYumDbContext _context;

        public CherishController(YumYumDbContext context)
        {
            _context = context;
        }

        public IActionResult Introduce()
		{
			return View();
		}

		public IActionResult Manage()
		{
			return View();
		}

		public IActionResult ManageAdd()
		{
			return View();
		}

		public IActionResult ManageEdit()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Match()
		{
            ViewBag.BreadcrumbsMatch = new List<BreadcrumbItem>{
			 new BreadcrumbItem("首頁", Url.Action("Index", "Recipe") ?? "#"),
			 new BreadcrumbItem("惜食專區", Url.Action("Introduce", "Cherish") ?? "#"),
			 new BreadcrumbItem("良食配對", "#") // 當前的頁面
             };

			var chrishOrders = from c in _context.CherishOrders
                               select new CherishMatch
                               {
                                   CherishId = c.CherishId,
                                   EndDate = c.EndDate,
                                   IngredAttributeName = c.IngredAttribute.IngredAttributeName,
                                   IngredientName = c.Ingredient.IngredientName,
                                   Quantity = c.Quantity,
                                   ObtainSource = c.ObtainSource,
                                   ObtainDate = c.ObtainDate,
                                   UserNickname = c.GiverUser.UserNickname!,
                                   CityName = c.CherishOrderInfo!.TradeCityKey,
                                   RegionName = c.CherishOrderInfo.TradeRegion.RegionName,
                                   ContactLine = c.CherishOrderInfo.ContactLine,
                                   ContactPhone = c.CherishOrderInfo.ContactPhone,
                                   ContactOther = c.CherishOrderInfo.ContactOther,
                                   CherishPhoto = c.CherishOrderCheck!.CherishPhoto,
                                   CherishValidDate = c.CherishOrderCheck.CherishValidDate  == null ? null : c.CherishOrderCheck.CherishValidDate.Value
                               };

            return View(await chrishOrders.ToListAsync());
		}

        [HttpPost]
        public IActionResult Match([Bind("IngredientSelect", "CitySelect", "RegionSelect")] CherishMatchSearch search )
        {
            ViewBag.BreadcrumbsMatch = new List<BreadcrumbItem>{
             new BreadcrumbItem("首頁", Url.Action("Index", "Recipe") ?? "#"),
             new BreadcrumbItem("惜食專區", Url.Action("Introduce", "Cherish") ?? "#"),
             new BreadcrumbItem("良食配對", "#") // 當前的頁面
             };

			var chrishSearchOrders = from c in _context.CherishOrders
						where (search.CityKey == null || c.CherishOrderInfo!.TradeCityKey == search.CityKey) &&
							  (search.RegionId > 0 || c.CherishOrderInfo!.TradeRegionId == search.RegionId) &&
							  (search.IngredientId > 0 || c.IngredientId == search.IngredientId)
						select new CherishMatch
						{
							CherishId = c.CherishId,
							EndDate = c.EndDate,
							IngredAttributeName = c.IngredAttribute.IngredAttributeName,
							IngredientName = c.Ingredient.IngredientName,
							Quantity = c.Quantity,
							ObtainSource = c.ObtainSource,
							ObtainDate = c.ObtainDate,
							UserNickname = c.GiverUser.UserNickname!,
							CityName = c.CherishOrderInfo!.TradeCityKey,
							RegionName = c.CherishOrderInfo.TradeRegion.RegionName,
							ContactLine = c.CherishOrderInfo.ContactLine,
							ContactPhone = c.CherishOrderInfo.ContactPhone,
							ContactOther = c.CherishOrderInfo.ContactOther
						};

            return View(chrishSearchOrders.ToListAsync());
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

        [HttpGet]
        public JsonResult GetRegions(string CityKey)
        {
            var regions = _context.Regions
                .Where(r => r.CityKey == CityKey)
                .Select(r => new { r.RegionId, r.RegionName })
                .ToList();

            return Json(regions);
        }

    }
}
