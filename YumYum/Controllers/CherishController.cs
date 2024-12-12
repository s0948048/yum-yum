﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.DiaSymReader;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using System.Data;
using System.Diagnostics;
using System.Linq;
using YumYum.Extension;
using YumYum.Models;
using YumYum.Models.DataTransferObject;
using YumYum.Models.ViewModels;
using static Azure.Core.HttpHeader;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace YumYum.Controllers
{
    public class CherishController : Controller
    {
        private readonly YumYumDbContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        public CherishController(YumYumDbContext context, ICompositeViewEngine viewEngine)
        {
            _context = context;
            _viewEngine = viewEngine;
        }

        public IActionResult Introduce()
        {
            // 設定 Breadcrumb
            ViewBag.Breadcrumbs = breadcrumbItems(this, [("惜食介紹", null)]);

            return View();
        }

        public IActionResult Manage()
        {
            if (HttpContext.Session.GetInt32("foreignId") is null)
            {
                if (HttpContext.Session.GetInt32("userId") is null)
                {
                    return RedirectToAction("LoginPage", "User");
                }
            }
            // 設定 Breadcrumb
            ViewBag.Breadcrumbs = breadcrumbItems(this, [("管理良食", null)]);

            // 抓到 userId (在 UserController LogInPage 這邊有set)
            int? userId = HttpContext.Session.GetInt32("userId");
            //int userId = 3238; // 登入者Id（測試用）

            ShowOrder((int)userId!);

            return View();
        }

        public IActionResult ShowOrder(int userId)
        {
            var query = from o in _context.CherishOrders
                        where o.GiverUserId == userId && o.TradeStateCode != 5
                        orderby o.SubmitDate descending
                        select new CherishOrderViewModel
                        {
                            // [訂單類]
                            CherishId = o.CherishId,
                            GiverUserId = o.GiverUserId,
                            SubmitDate = o.SubmitDate,
                            Quantity = o.Quantity,
                            EndDate = o.EndDate,
                            ObtainSource = o.ObtainSource,
                            ObtainDate = o.ObtainDate,
                            CherishValidDate = o.CherishOrderCheck!.CherishValidDate,
                            CherishPhoto = o.CherishOrderCheck.CherishPhoto,

                            // [食材類]
                            IngredientName = o.Ingredient.IngredientName,
                            IngredAttributeName = o.IngredAttribute.IngredAttributeName,

                            // [地區類]
                            TradeCityKey = o.CherishOrderInfo!.TradeCityKey,
                            CityName = o.CherishOrderInfo!.TradeCityKeyNavigation.CityName,
                            TradeRegionId = o.CherishOrderInfo!.TradeRegionId,
                            RegionName = o.CherishOrderInfo.TradeRegion.RegionName,

                            // [聯絡類]
                            //UserNickname = o.CherishOrderInfo.UserNickname,
                            ContactLine = o.CherishOrderInfo.ContactLine,
                            ContactPhone = o.CherishOrderInfo.ContactPhone,
                            ContactOther = o.CherishOrderInfo.ContactOther,
                            //TradeTimeCode

                            // [訂單狀態類]
                            TradeStateCode = o.TradeStateCode,
                            ReasonId = o.CherishOrderCheck.ReasonId
                        };

            List<CherishOrderViewModel> orderList = query.ToList();
            return PartialView("_PartialOrder", orderList);
        }

        public IActionResult ManageAdd()
        {
            // 設定 Breadcrumb
            ViewBag.Breadcrumbs = breadcrumbItems(this, [("新增良食", null)]);

            int? userId = HttpContext.Session.GetInt32("userId");
            ViewBag.UserId = userId;

            var ingredientType = from o in _context.IngredAttributes
                                 where o.IngredAttributeId == 1 || o.IngredAttributeId == 4 || o.IngredAttributeId == 5 || o.IngredAttributeId == 6
                                 select o;
            ViewBag.IngredientTypeList = ingredientType;

            var city = from o in _context.Cities
                       select o;
            ViewBag.CityList = city;

            ViewBag.Days = GetDay();
            ViewBag.Times = GetTime();

            return View();
        }

        public IActionResult ManageEdit(int cherishId)
        {
            // 設定 Breadcrumb
            ViewBag.Breadcrumbs = breadcrumbItems(this, [("編輯良食", null)]);

            var ingredientType = from o in _context.IngredAttributes
                                 where o.IngredAttributeId == 1 || o.IngredAttributeId == 4 || o.IngredAttributeId == 5 || o.IngredAttributeId == 6
                                 select o;
            ViewBag.IngredientTypeList = ingredientType;

            var ingredient = from o in _context.Ingredients
                             select o;
            ViewBag.IngredientList = ingredient;

            var city = from o in _context.Cities
                       select o;
            ViewBag.CityList = city;

            var region = from o in _context.Regions
                         select o;
            ViewBag.RegionList = region;

            CherishOrderViewModel orderDetail = ShowOrderDetail(cherishId);

            ViewBag.Days = GetDay();
            ViewBag.Times = GetTime();
            ViewBag.TradeTimes = GetTradeTime(orderDetail.GiverUserId);

            return View(orderDetail);
        }

        [HttpPost]
        public IActionResult ManageAdd(CherishOrder order, CherishOrderCheck check, CherishOrderInfo info)
        //, CherishTradeTime tradeTime
        {
            try
            {
                // 儲存訂單資料
                // 取得現在時間
                DateTime current = DateTime.Now;
                // 將 DateTime 轉換為 DateOnly
                DateOnly submitDate = DateOnly.FromDateTime(current);
                order.SubmitDate = submitDate;
                order.TradeStateCode = 1;
                _context.CherishOrders.Add(order);
                _context.SaveChanges(); // 確保訂單被成功插入並且取得ID

                // 設定和儲存訂單檢查資料，並且關聯到訂單
                check.CherishId = order.CherishId; // CherishId 是訂單的主鍵
                check.CherishPhoto = "ABC";
                check.OtherPhoto = "DEF";
                _context.CherishOrderChecks.Add(check);
                _context.SaveChanges(); // 確保檢查資料被成功插入

                // 設置和儲存訂單明細，並且關聯到訂單
                info.CherishId = order.CherishId;
                _context.CherishOrderInfos.Add(info);
                _context.SaveChanges();

                // 設置和儲存面交時段資料，並且關聯到訂單
                //tradeTime.CherishId = order.CherishId;
                //_context.CherishTradeTimes.Add(tradeTime);
                //_context.SaveChanges();

                // 如果成功，重新導向或返回確認訊息
                return RedirectToAction("Manage");
            }
            catch (Exception ex)
            {
                // 如果出現錯誤，前往錯誤頁面
                return View("Error");
            }
        }

        [HttpPost] // no use
        public async Task<IActionResult> UploadImage(IFormFile image1, IFormFile image2, IFormFile image3)
        {
            // 依照傳入的檔案，選擇處理對應的圖片
            IFormFile photo = null;

            if (image1 != null)
            {
                photo = image1;
            }
            else if (image2 != null)
            {
                photo = image2;
            }
            else if (image3 != null)
            {
                photo = image3;
            }

            // 檢查是否有檔案被選擇
            if (photo != null)
            {
                // 設定儲存檔案的路徑
                string filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img/cherish", photo.FileName);

                // 確保目錄存在
                string? directory = Path.GetDirectoryName(filepath);
                if (!Directory.Exists(directory)) // 如果目錄不存在，會得到 false，前方有! 則變成 true。
                {
                    Directory.CreateDirectory(filepath);
                }

                // 儲存檔案
                using (var stream = new FileStream(filepath, FileMode.Create)) // 使用了 using 關鍵字，表示這個物件會在 using 區塊結束後自動釋放資源。
                {
                    await photo.CopyToAsync(stream); // 將上傳的影像檔案（photo）的內容複製到剛才創建的 FileStream（即 stream）中。
                }

                // 返回成功的訊息
                ViewBag.Message = "照片上傳成功！";
            }
            else
            {
                ViewBag.Message = "未選擇照片！";
            }

            // 返回視圖（可以顯示訊息或重新顯示表單）
            return View("ManageAdd");
        }

        [HttpPost] // no use
        public async Task<IActionResult> SendoutOrder(int orderId, int orderState)
        {
            var query = from o in _context.CherishOrders
                        where o.CherishId == orderId
                        select o;
            var data = query.Single();

            if (data.TradeStateCode == 0)
            {
                data.TradeStateCode = 5;
                _context.Update(data);
                await _context.SaveChangesAsync();
                return RedirectToAction("Manage", "Cherish");// bug 頁面沒刷新
            }
            return NotFound();
        }

        //--------------------------   彥廷  首   -------------------------

        [HttpGet]
        public async Task<IActionResult> Match()
        {
            ViewBag.Breadcrumbs = breadcrumbItems(this, [("良食配對", null)]);

            ViewBag.city = new SelectList(_context.Cities, "CityKey", "CityName");

            List<int> cherishAttr = [1, 4, 5, 6];

            var chrishOrders = await _context.CherishOrders
                    .Where(c => cherishAttr.Contains(c.IngredAttributeId))
                    .Where(c => c.EndDate > DateOnly.FromDateTime(DateTime.Now))
                    .Where(c => c.TradeStateCode == 0)
                    .MatchInNavigation()
                    .OrderBy(c => c.EndDate)
                    .Select(c => ObjectToCherishMatchDTO(c, _context))
                    .ToListAsync();




            return View(chrishOrders);
        }

        [HttpPost] // 提交聯絡資訊
        public async Task<IActionResult> ApplyCherish([FromForm] CherishOrderApplicant SumitUser)
        {
            int? aId = HttpContext.Session.GetInt32("userId");

            if (SumitUser.ApplicantContactLine is null
                && SumitUser.ApplicantContactPhone is null
                && SumitUser.ApplicantContactOther is null)
            {
                return new BadRequestObjectResult(new { success = false, message = "必須傳入最少一種聯絡方式！" });
            }

            var check = _context.CherishOrderApplicants.Any(o => o.CherishId == SumitUser.CherishId && o.ApplicantId == aId);
            if (check)
            {
                return new BadRequestObjectResult(new { success = false, message = "已申請過！！" });
            }

            var od = new CherishOrderApplicant
            {
                CherishId = SumitUser.CherishId,
                ApplicantId = (int)aId!,
                UserNickname = SumitUser.UserNickname,
                ApplicantContactLine = SumitUser.ApplicantContactLine,
                ApplicantContactPhone = SumitUser.ApplicantContactPhone,
                ApplicantContactOther = SumitUser.ApplicantContactOther
            };

            await _context.CherishOrderApplicants.AddAsync(od);
            await _context.SaveChangesAsync();


            var orderDetail = await _context.CherishOrders
                    .Where(c => c.CherishId == SumitUser.CherishId)
                    .Where(c => c.TradeStateCode == 0)
                    .MatchInNavigation()
                    .Select(c => ObjectToCherishMatchDTO(c, _context))
                    .FirstAsync();

            // 這裡是面交時間
            var timeSpan = await (from d in _context.CherishTradeTimes
                                  where d.CherishId == SumitUser.CherishId
                                  select d).ToListAsync();

            ViewBag.Mon = timeSpan.Where(d => d.TradeTimeCode.Contains("Mon"));
            ViewBag.Tue = timeSpan.Where(d => d.TradeTimeCode.Contains("Tue"));
            ViewBag.Wes = timeSpan.Where(d => d.TradeTimeCode.Contains("Wes"));
            ViewBag.Thr = timeSpan.Where(d => d.TradeTimeCode.Contains("Thr"));
            ViewBag.Fri = timeSpan.Where(d => d.TradeTimeCode.Contains("Fri"));
            ViewBag.Sat = timeSpan.Where(d => d.TradeTimeCode.Contains("Sat"));
            ViewBag.Sun = timeSpan.Where(d => d.TradeTimeCode.Contains("Sun"));


            var pv = GetPartialHtml("_Partial_GiverInfo",orderDetail);

            return Ok(new { success = true, message = "申請成功!", view = pv });
        }

        // 2024-11-25 
        [HttpPost] // 提供
        public async Task<IActionResult> ContactOrder(int cherishID)
        {
            // 這裡是面交時間
            var timeSpan = await (from d in _context.CherishTradeTimes
                                  where d.CherishId == cherishID
                                  select d).ToListAsync();

            ViewBag.Mon = timeSpan.Where(d => d.TradeTimeCode.Contains("Mon"));
            ViewBag.Tue = timeSpan.Where(d => d.TradeTimeCode.Contains("Tue"));
            ViewBag.Wes = timeSpan.Where(d => d.TradeTimeCode.Contains("Wes"));
            ViewBag.Thr = timeSpan.Where(d => d.TradeTimeCode.Contains("Thr"));
            ViewBag.Fri = timeSpan.Where(d => d.TradeTimeCode.Contains("Fri"));
            ViewBag.Sat = timeSpan.Where(d => d.TradeTimeCode.Contains("Sat"));
            ViewBag.Sun = timeSpan.Where(d => d.TradeTimeCode.Contains("Sun"));

            var orderDetail = await _context.CherishOrders
                    .Where(c => c.CherishId == cherishID)
                    .Where(c => c.TradeStateCode == 0)
                    .MatchInNavigation()
                    .Select(c => ObjectToCherishMatchDTO(c, _context))
                    .FirstAsync();


            return PartialView("_Partial_GiverInfo", orderDetail);
        }

        // 2024-11-25 
        [HttpGet]
        public async Task<IActionResult> DetailOrder(int cherishID)
        {


            // 這裡是面交時間
            var timeSpan = await (from d in _context.CherishTradeTimes
                                  where d.CherishId == cherishID
                                  select d).ToListAsync();

            ViewBag.Mon = timeSpan.Where(d => d.TradeTimeCode.Contains("Mon"));
            ViewBag.Tue = timeSpan.Where(d => d.TradeTimeCode.Contains("Tue"));
            ViewBag.Wes = timeSpan.Where(d => d.TradeTimeCode.Contains("Wes"));
            ViewBag.Thr = timeSpan.Where(d => d.TradeTimeCode.Contains("Thr"));
            ViewBag.Fri = timeSpan.Where(d => d.TradeTimeCode.Contains("Fri"));
            ViewBag.Sat = timeSpan.Where(d => d.TradeTimeCode.Contains("Sat"));
            ViewBag.Sun = timeSpan.Where(d => d.TradeTimeCode.Contains("Sun"));


            var orderDetail = await _context.CherishOrders
                .Where(c => c.CherishId == cherishID)
                .Where(c => c.TradeStateCode == 0)
                .MatchInNavigation()
                .Select(c => ObjectToCherishMatchDTO(c, _context))
                .FirstAsync();

            return PartialView("_Partial_DetailMatch", orderDetail);
        }

        // 2024-11-25 
        [HttpPost]
        public async Task<IActionResult> SortCherish([FromBody] CherishFilter f)
        {
            IQueryable<CherishOrder> query = _context.CherishOrders;

            List<int> cherishAttr = [1, 4, 5, 6];
            query = query.Where(c => cherishAttr.Contains(c.IngredAttributeId));
            query = query.Where(c => c.EndDate > DateOnly.FromDateTime(DateTime.Now));

            if (f.SortAttr != null && f.SortAttr.Count != 0)
            {
                query = query.Where(o => f.SortAttr.Contains(o.IngredAttributeId));
            }

            if (f.SortCont != null && f.SortCont.Count != 0)
            {
                query = query.Where(o =>
                    (f.SortCont.Contains(1) && o.CherishOrderInfo!.ContactLine != null) ||
                    (f.SortCont.Contains(2) && o.CherishOrderInfo!.ContactPhone != null) ||
                    (f.SortCont.Contains(3) && o.CherishOrderInfo!.ContactOther != null));
            }

            if (f.SortDay != null && f.SortDay.Count != 0)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                query = query.Where(o =>
                    (f.SortDay.Contains(1) && o.EndDate < today.AddDays(1)) ||
                    (f.SortDay.Contains(2) && o.EndDate >= today.AddDays(1) && o.EndDate < today.AddDays(3)) ||
                    (f.SortDay.Contains(3) && o.EndDate >= today.AddDays(3) && o.EndDate < today.AddDays(7)) ||
                    (f.SortDay.Contains(4) && o.EndDate >= today.AddDays(7)));
            }


            if (!string.IsNullOrEmpty(f.Search!.CitySelect))
                query = query.Where(c => c.CherishOrderInfo!.TradeCityKey == f.Search.CitySelect);

            if (f.Search.RegionSelect > 0)
                query = query.Where(c => c.CherishOrderInfo!.TradeRegionId == f.Search.RegionSelect);

            if (!string.IsNullOrEmpty(f.Search.IngredientSelect))
                query = query.Where(c => c.Ingredient.IngredientName.Contains(f.Search.IngredientSelect));

            query = query.Include(c => c.CherishOrderInfo);

            var chrishSearchOrders = await QResult(query);

            return PartialView("_Partial_Sorting", chrishSearchOrders);
        }



        //--------------------------   彥廷  尾   -------------------------




        [HttpGet]
        [Route("Cherish/MatchHistory")]
        public async Task<IActionResult> MatchHistory()
        {
            if (HttpContext.Session.GetInt32("foreignId") is null)
            {
                if (HttpContext.Session.GetInt32("userId") is null)
                {
                    return RedirectToAction("LoginPage", "User");
                }
            }
            // 設定 Breadcrumb
            ViewBag.Breadcrumbs = breadcrumbItems(this, [("配對紀錄", "MatchHistoryMine"), ("我的食材", null)]);


            var userId = HttpContext.Session.GetInt32("userId");
            var chrishOrders = from c in _context.CherishOrders
                               where c.GiverUserId == userId
                               select new MatchHistory
                               {
                                   CherishId = c.CherishId,
                                   GiverUserId = c.GiverUserId,
                                   EndDate = c.EndDate,
                                   IngredAttributeName = c.IngredAttribute.IngredAttributeName,
                                   IngredientName = c.Ingredient.IngredientName,
                                   Quantity = c.Quantity,
                                   ObtainSource = c.ObtainSource,
                                   ObtainDate = c.ObtainDate,
                                   UserNickname = c.GiverUser.UserNickname!,
                                   CityName = c.CherishOrderInfo!.TradeCityKeyNavigation.CityName,
                                   RegionName = c.CherishOrderInfo.TradeRegion.RegionName,
                                   ContactLine = c.CherishOrderInfo.ContactLine,
                                   ContactPhone = c.CherishOrderInfo.ContactPhone,
                                   ContactOther = c.CherishOrderInfo.ContactOther,
                                   CherishPhoto = c.CherishOrderCheck!.CherishPhoto,
                                   CherishValidDate = c.CherishOrderCheck.CherishValidDate == null ? null : c.CherishOrderCheck.CherishValidDate.Value,
                                   IsMine = c.GiverUserId == userId // 判斷是否是 "我的食材"
                               };

            return View(await chrishOrders.ToListAsync());
        }



        public async Task<IActionResult> MatchHistoryOthers()
        {
            // 設定 Breadcrumb
            ViewBag.Breadcrumbs = breadcrumbItems(this, [("配對紀錄", "MatchHistoryOthers"), ("別人的食材", "#")]);

            var userId = HttpContext.Session.GetInt32("userId");
            var chrishOrders = from c in _context.CherishOrders
                               join coa in _context.CherishOrderApplicants
                               on c.CherishId equals coa.CherishId
                               where coa.ApplicantId == userId
                               select new MatchHistory
                               {

                                   ApplicantId = coa.ApplicantId,
                                   CherishId = c.CherishId,
                                   GiverUserId = c.GiverUserId,
                                   EndDate = c.EndDate,
                                   IngredAttributeName = c.IngredAttribute.IngredAttributeName,
                                   IngredientName = c.Ingredient.IngredientName,
                                   Quantity = c.Quantity,
                                   ObtainSource = c.ObtainSource,
                                   ObtainDate = c.ObtainDate,
                                   UserNickname = c.GiverUser.UserNickname!,
                                   CityName = c.CherishOrderInfo!.TradeCityKeyNavigation.CityName,
                                   RegionName = c.CherishOrderInfo.TradeRegion.RegionName,
                                   ContactLine = c.CherishOrderInfo.ContactLine,
                                   ContactPhone = c.CherishOrderInfo.ContactPhone,
                                   ContactOther = c.CherishOrderInfo.ContactOther,
                                   CherishPhoto = c.CherishOrderCheck!.CherishPhoto,
                                   CherishValidDate = c.CherishOrderCheck.CherishValidDate == null ? null : c.CherishOrderCheck.CherishValidDate.Value
                               };

            return View(await chrishOrders.ToListAsync());
        }

        [Route("Cherish/MatchHistoryMineInfo/{cherishId}")]
        public async Task<IActionResult> MatchHistoryMineInfo(int cherishId)
        {
            // 設定 Breadcrumb
            ViewBag.Breadcrumbs = new List<BreadcrumbItem>{
             new BreadcrumbItem("首頁", Url.Action("Index", "Recipe") ?? "#"),
             new BreadcrumbItem("惜食專區", Url.Action("Introduce", "Cherish") ?? "#"),
             new BreadcrumbItem("配對紀錄", Url.Action("MatchHistory", "Cherish") ?? "#"),
             new BreadcrumbItem("我的食材", "#") // 當前的頁面
             };

            var userId = HttpContext.Session.GetInt32("userId");
            var chrishOrders = from c in _context.CherishOrders
                               join coa in _context.CherishOrderApplicants
                               on c.CherishId equals coa.CherishId
                               join ub in _context.UserBios
                               on coa.ApplicantId equals ub.UserId
                               where c.GiverUserId == userId && c.CherishId == cherishId
                               select new MatchHistory
                               {
                                   UserNickname = coa.UserNickname,
                                   ApplicantId = coa.ApplicantId,
                                   ApplicantContactLine = coa.ApplicantContactLine,
                                   ApplicantContactPhone = coa.ApplicantContactPhone,
                                   ApplicantContactOther = coa.ApplicantContactOther,
                                   CherishId = c.CherishId,
                                   GiverUserId = c.GiverUserId,
                                   EndDate = c.EndDate,
                                   IngredAttributeName = c.IngredAttribute.IngredAttributeName,
                                   IngredientName = c.Ingredient.IngredientName,
                                   Quantity = c.Quantity,
                                   ObtainSource = c.ObtainSource,
                                   ObtainDate = c.ObtainDate,
                                   CityName = c.CherishOrderInfo!.TradeCityKeyNavigation.CityName,
                                   RegionName = c.CherishOrderInfo.TradeRegion.RegionName,
                                   ContactLine = c.CherishOrderInfo.ContactLine,
                                   ContactPhone = c.CherishOrderInfo.ContactPhone,
                                   ContactOther = c.CherishOrderInfo.ContactOther,
                                   CherishPhoto = c.CherishOrderCheck!.CherishPhoto,
                                   CherishValidDate = c.CherishOrderCheck.CherishValidDate,
                                   HeadShot = ub.HeadShot
                               };

            var list = await chrishOrders.ToListAsync();

            // 偵錯: 檢查查詢結果是否有資料
            if (!list.Any())
            {
                Console.WriteLine($"No data found for cherishId {cherishId} and userId {userId}");
            }

            return View(list);
        }




        [HttpGet]
        [Route("Cherish/MatchHistorySearch")]
        public async Task<IActionResult> MatchHistorySearch(string query)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            // 根據查詢條件篩選數據
            var results = from c in _context.CherishOrders
                          join ig in _context.Ingredients
                          on c.IngredientId equals ig.IngredientId
                          where c.GiverUserId == userId &&
                                (string.IsNullOrEmpty(query) ||
                                 ig.IngredientName.Contains(query)) // 查詢食材名稱中包含 query
                          select new MatchHistory
                          {
                              CherishId = c.CherishId,
                              GiverUserId = c.GiverUserId,
                              EndDate = c.EndDate,
                              IngredAttributeName = c.IngredAttribute.IngredAttributeName,
                              IngredientName = ig.IngredientName,
                              Quantity = c.Quantity,
                              ObtainSource = c.ObtainSource,
                              ObtainDate = c.ObtainDate,
                              UserNickname = c.GiverUser.UserNickname!,
                              CityName = c.CherishOrderInfo!.TradeCityKeyNavigation.CityName,
                              RegionName = c.CherishOrderInfo.TradeRegion.RegionName,
                              ContactLine = c.CherishOrderInfo.ContactLine,
                              ContactPhone = c.CherishOrderInfo.ContactPhone,
                              ContactOther = c.CherishOrderInfo.ContactOther,
                              CherishPhoto = c.CherishOrderCheck!.CherishPhoto,
                              CherishValidDate = c.CherishOrderCheck.CherishValidDate,
                              IsMine = c.GiverUserId == userId // 判斷是否是 "我的食材"
                          };

            return PartialView("_MatchHistorySearchResults", await results.ToListAsync());
        }

        public async Task<IActionResult> FilterMatchHistory(int filterType)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            IQueryable<MatchHistory> matchHistories;

            if (filterType == 1) // 我的食材
            {
                matchHistories = from c in _context.CherishOrders
                                 where c.GiverUserId == userId
                                 select new MatchHistory
                                 {
                                     CherishId = c.CherishId,
                                     GiverUserId = c.GiverUserId,
                                     EndDate = c.EndDate,
                                     IngredAttributeName = c.IngredAttribute.IngredAttributeName,
                                     IngredientName = c.Ingredient.IngredientName,
                                     Quantity = c.Quantity,
                                     ObtainSource = c.ObtainSource,
                                     ObtainDate = c.ObtainDate,
                                     UserNickname = c.GiverUser.UserNickname!,
                                     CityName = c.CherishOrderInfo!.TradeCityKeyNavigation.CityName,
                                     RegionName = c.CherishOrderInfo.TradeRegion.RegionName,
                                     ContactLine = c.CherishOrderInfo.ContactLine,
                                     ContactPhone = c.CherishOrderInfo.ContactPhone,
                                     ContactOther = c.CherishOrderInfo.ContactOther,
                                     CherishPhoto = c.CherishOrderCheck!.CherishPhoto,
                                     CherishValidDate = c.CherishOrderCheck.CherishValidDate
                                 };
            }
            else if (filterType == 2) // 別人的食材
            {
                matchHistories = from c in _context.CherishOrders
                                 join coa in _context.CherishOrderApplicants
                                 on c.CherishId equals coa.CherishId
                                 where coa.ApplicantId == userId
                                 select new MatchHistory
                                 {
                                     ApplicantId = coa.ApplicantId,
                                     CherishId = c.CherishId,
                                     GiverUserId = c.GiverUserId,
                                     EndDate = c.EndDate,
                                     IngredAttributeName = c.IngredAttribute.IngredAttributeName,
                                     IngredientName = c.Ingredient.IngredientName,
                                     Quantity = c.Quantity,
                                     ObtainSource = c.ObtainSource,
                                     ObtainDate = c.ObtainDate,
                                     UserNickname = c.GiverUser.UserNickname!,
                                     CityName = c.CherishOrderInfo!.TradeCityKeyNavigation.CityName,
                                     RegionName = c.CherishOrderInfo.TradeRegion.RegionName,
                                     ContactLine = c.CherishOrderInfo.ContactLine,
                                     ContactPhone = c.CherishOrderInfo.ContactPhone,
                                     ContactOther = c.CherishOrderInfo.ContactOther,
                                     CherishPhoto = c.CherishOrderCheck!.CherishPhoto,
                                     CherishValidDate = c.CherishOrderCheck.CherishValidDate
                                 };
            }
            else
            {
                return BadRequest("Invalid filter type.");
            }

            var results = await matchHistories.ToListAsync();
            return PartialView("_MatchHistorySearchResults", results); // 返回部分視圖
        }



        [Route("Cherish/MatchHistoryOthersInfo/{cherishId}")]
        public async Task<IActionResult> MatchHistoryOthersInfo(int cherishId)
        {

            // 設定 Breadcrumb
            ViewBag.Breadcrumbs = new List<BreadcrumbItem>{
             new BreadcrumbItem("首頁", Url.Action("Index", "Recipe") ?? "#"),
             new BreadcrumbItem("惜食專區", Url.Action("Introduce", "Cherish") ?? "#"),
             new BreadcrumbItem("配對紀錄", Url.Action("MatchHistoryOthers", "Cherish") ?? "#"),
             new BreadcrumbItem("別人的食材", "#") // 當前的頁面
             };

            //設定可面交時間

            // 先抓到該筆訂單or該預設聯絡資訊的所有面交時段
            var timeSpan = await (from d in _context.CherishTradeTimes
                                  where d.CherishId == cherishId
                                  select d).ToListAsync();

            // 進行篩選，並傳遞給ViewBag
            ViewBag.Mon = timeSpan.Where(d => d.TradeTimeCode.Contains("Mon"));
            ViewBag.Tue = timeSpan.Where(d => d.TradeTimeCode.Contains("Tue"));
            ViewBag.Wes = timeSpan.Where(d => d.TradeTimeCode.Contains("Wes"));
            ViewBag.Thr = timeSpan.Where(d => d.TradeTimeCode.Contains("Thr"));
            ViewBag.Fri = timeSpan.Where(d => d.TradeTimeCode.Contains("Fri"));
            ViewBag.Sat = timeSpan.Where(d => d.TradeTimeCode.Contains("Sat"));
            ViewBag.Sun = timeSpan.Where(d => d.TradeTimeCode.Contains("Sun"));


            var chrishOrders = from c in _context.CherishOrders
                               join coa in _context.CherishOrderApplicants
                               on c.CherishId equals coa.CherishId
                               where c.CherishId == cherishId
                               select new MatchHistory
                               {
                                   ApplicantId = coa.ApplicantId,
                                   CherishId = c.CherishId,
                                   GiverUserId = c.GiverUserId,
                                   EndDate = c.EndDate,
                                   IngredAttributeName = c.IngredAttribute.IngredAttributeName,
                                   IngredientName = c.Ingredient.IngredientName,
                                   Quantity = c.Quantity,
                                   ObtainSource = c.ObtainSource,
                                   ObtainDate = c.ObtainDate,
                                   UserNickname = c.GiverUser.UserNickname!,
                                   CityName = c.CherishOrderInfo!.TradeCityKeyNavigation.CityName,
                                   RegionName = c.CherishOrderInfo.TradeRegion.RegionName,
                                   ContactLine = c.CherishOrderInfo.ContactLine,
                                   ContactPhone = c.CherishOrderInfo.ContactPhone,
                                   ContactOther = c.CherishOrderInfo.ContactOther,
                                   CherishPhoto = c.CherishOrderCheck!.CherishPhoto,
                                   CherishValidDate = c.CherishOrderCheck.CherishValidDate == null ? null : c.CherishOrderCheck.CherishValidDate.Value
                               };





            return View(await chrishOrders.ToListAsync());
        }

        [HttpGet]
        public IActionResult ContactInformation()
        {

            if (HttpContext.Session.GetInt32("userId") is null) return RedirectToAction("LoginPage", "User");
            int? userId = HttpContext.Session.GetInt32("userId");

            // 設定 Breadcrumb
            ViewBag.Breadcrumbs = new List<BreadcrumbItem>{
             new BreadcrumbItem("首頁", Url.Action("Index", "Cherish") ?? "#"),
             new BreadcrumbItem("惜食專區", Url.Action("Introduce", "Cherish") ?? "#"),
             new BreadcrumbItem("聯絡資料", "#") // 當前的頁面
             };

            ViewBag.CityList = _context.Cities.Select(c => c);
            ViewBag.RegionList = _context.Regions.Select(c => c);


            ViewBag.Days = GetDay();
            ViewBag.Times = GetTime();
            ViewBag.TradeTimes = GetTradeTime((int)userId!);


            var contact = from o in _context.CherishDefaultInfos
                          where o.GiverUserId == userId
                          select ObjecToCherishContactView(o, _context);

            if (contact.Any())
            {
                return View(contact.Single());
            }
            else
            {
                return View(EmptyContact((int)userId));
            }


        }



        [HttpPost]
        public async Task<IActionResult> ContactInformation(List<String> AvailableTime, [FromForm] CherishDefaultInfo user)
        {
            /* ---------------------------【 右側時段修改 】--------------------------*/
            // 找到目前資料庫所有時段，方便稍後做比較
            IEnumerable<string> exsistTime = _context.CherishDefaultTimeSets.Where(o => o.GiverUserId == user.GiverUserId).Select(o => o.TradeTimeCode).ToList();
            //GetTradeTime(user.GiverUserId).ToList();

            // Except
            // 新的比舊的還多出的 => 要新增
            var addTime = AvailableTime.Except(exsistTime);

            // 舊的比新的還多出的 => 要刪除的
            var delTime = exsistTime.Except(AvailableTime);

            // 執行刪除
            _context.CherishDefaultTimeSets.RemoveRange(
                    _context.CherishDefaultTimeSets.Where(o => o.GiverUserId == user.GiverUserId && delTime.Contains(o.TradeTimeCode))
                    );

            // 執行新增
            foreach (var time in addTime)
            {
                _context.CherishDefaultTimeSets.Add(new CherishDefaultTimeSet()
                {
                    GiverUserId = user.GiverUserId,
                    TradeTimeCode = time
                });
            }

            // 完成修改
            _context.SaveChanges();

            /* ---------------------------【 左側資料修改 】--------------------------*/

            if (_context.CherishDefaultInfos.Any(co => co.GiverUserId == user.GiverUserId))
            {
                _context.Update(user);
            }
            else
            {
                _context.CherishDefaultInfos.Add(user);
            }



            await _context.SaveChangesAsync();
            return RedirectToAction("ContactInformation");
            // bug asp-* 驗證好像只做半套
        }



        [HttpGet]
        public IActionResult GetRegions(string CityKey)
        {
            var regions = _context.Regions
                .Where(r => r.CityKey == CityKey)
                .ToList();

            return PartialView("_PartialRegion", regions);
        }





        /* --------------------------------------------------------------------------------*/
        /* --------------------------------------------------------------------------------*/
        /* --------------------------------------------------------------------------------*/
        /* --------------------------------------------------------------------------------*/
        /* --------------------------------------------------------------------------------*/
        [HttpGet]
        public IActionResult GetIngredients(string IngredAttributeId)
        {
            var ingredients = _context.Ingredients
                .Where(o => o.AttributionId.ToString() == IngredAttributeId)
                .ToList();

            return PartialView("_PartialIngredient", ingredients);
        }

        public Dictionary<string, string> GetDay()
        {
            // 星期幾 Key-Value Pair
            Dictionary<string, string> day = new Dictionary<string, string>
            {
                {"Mon","星期一" },
                {"Tue","星期二" },
                {"Wes","星期三" },
                {"Thr","星期四" },
                {"Fri","星期五" },
                {"Sat","星期六" },
                {"Sun","星期日" }
            };
            return day;
        }

        public Dictionary<int, string> GetTime()
        {
            // 時段 Key-Value Pair
            Dictionary<int, string> time = new Dictionary<int, string>
            {
                { 1, "早上" },
                { 2, "下午" },
                { 3, "晚上" }
            };
            return time;
        }

        public List<string> GetTradeTime(int userId)
        {
            if (userId <= 0)
            {
                return [" "];
            }
            var query = from o in _context.CherishDefaultTimeSets
                        where o.GiverUserId == userId
                        select o.TradeTimeCode;
            var result = query.ToList();

            return result;
        }

        public CherishOrderViewModel ShowOrderDetail(int cherishId)
        {
            var query = from o in _context.CherishOrders
                        where o.CherishId == cherishId && o.TradeStateCode != 5
                        select new CherishOrderViewModel
                        {
                            // [訂單類]
                            CherishId = o.CherishId,
                            GiverUserId = o.GiverUserId,
                            SubmitDate = o.SubmitDate,
                            Quantity = o.Quantity,
                            EndDate = o.EndDate,
                            ObtainSource = o.ObtainSource,
                            ObtainDate = o.ObtainDate,
                            CherishValidDate = o.CherishOrderCheck!.CherishValidDate,
                            CherishPhoto = o.CherishOrderCheck.CherishPhoto,

                            // [食材類]
                            IngredientName = o.Ingredient.IngredientName,
                            IngredAttributeName = o.IngredAttribute.IngredAttributeName,

                            // [地區類]
                            TradeCityKey = o.CherishOrderInfo!.TradeCityKey,
                            CityName = o.CherishOrderInfo!.TradeCityKeyNavigation.CityName,
                            TradeRegionId = o.CherishOrderInfo!.TradeRegionId,
                            RegionName = o.CherishOrderInfo.TradeRegion.RegionName,

                            // [聯絡類]
                            UserNickname = o.CherishOrderInfo.UserNickname,
                            ContactLine = o.CherishOrderInfo.ContactLine,
                            ContactPhone = o.CherishOrderInfo.ContactPhone,
                            ContactOther = o.CherishOrderInfo.ContactOther,
                            //TradeTimeCode

                            // [訂單狀態類]
                            TradeStateCode = o.TradeStateCode,
                            ReasonId = o.CherishOrderCheck.ReasonId
                        };

            CherishOrderViewModel orderDetail = query.Single();
            return orderDetail;
        }

        private static CherishContactViewModel ObjecToCherishContactView(CherishDefaultInfo o, YumYumDbContext _dbContext)
        {
            o.TradeCityKeyNavigation = _dbContext.Cities.Where(c => c.CityKey == o.TradeCityKey).First();
            o.TradeRegion = _dbContext.Regions.Where(r => r.RegionId == o.TradeRegionId).First();

            return new CherishContactViewModel
            {
                GiverUserId = o.GiverUserId,
                UserNickname = o.UserNickname,
                TradeCityKey = o.TradeCityKey,
                CityName = o.TradeCityKeyNavigation.CityName,
                TradeRegionId = o.TradeRegionId,
                RegionName = o.TradeRegion.RegionName,
                ContactLine = o.ContactLine,
                ContactPhone = o.ContactPhone,
                ContactOther = o.ContactOther,
            };
        }

        private static CherishMatch ObjectToCherishMatchDTO(CherishOrder c, YumYumDbContext _context)
        {
            return new CherishMatch
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
                CherishValidDate = c.CherishOrderCheck.CherishValidDate == null ? null : c.CherishOrderCheck.CherishValidDate.Value
            };
        }

        private CherishContactViewModel EmptyContact(int userId)
        {
            return new CherishContactViewModel
            {
                GiverUserId = userId,
                UserNickname = "",
                TradeCityKey = "",
                CityName = "",
                TradeRegionId = 0,
                RegionName = "",
                ContactLine = "",
                ContactPhone = "",
                ContactOther = "",
            };
        }

        public async Task<List<CherishMatch>> QResult(IQueryable<CherishOrder> query)
        {
            var c = await query.Where(c => c.TradeStateCode == 0)
                    .OrderBy(c => c.EndDate)
                    .MatchInNavigation()
                    .Select(c => ObjectToCherishMatchDTO(c, _context))
                    .ToListAsync();

            return c;
        }

        private static List<BreadcrumbItem> breadcrumbItems(Controller c, (string, string?)[] li)
        {
            var x = new List<BreadcrumbItem>()
            {
                new BreadcrumbItem("首頁", c.Url.Action("Index", "Recipe") ?? "#"),
                new BreadcrumbItem("惜食專區", c.Url.Action("Introduce", "Cherish") ?? "#"),
            };

            string contName = "cherish";
            foreach (var l in li)
            {
                x.Add(new BreadcrumbItem(l.Item1, c.Url.Action(l.Item2, contName) ?? "#"));
            }

            return x;

        }

        async private Task<string> GetPartialHtml(string viewString, object model)
        {
            ViewData.Model = model;

            using (var writer = new StringWriter())
            {

                var viewResult = _viewEngine.FindView(ControllerContext, viewString, false);

                var viewContext = new ViewContext(
                     ControllerContext,
                     viewResult.View!,
                     ViewData,
                     TempData,
                     writer,
                     new HtmlHelperOptions()
                 );

                await viewResult.View!.RenderAsync(viewContext);
                return writer.GetStringBuilder().ToString();
            }
        }

    }
}
