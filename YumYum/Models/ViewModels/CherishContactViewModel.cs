using System.ComponentModel.DataAnnotations;

namespace YumYum.Models.ViewModels
{
	public class CherishContactViewModel
	{
		public int GiverUserId { get; set; }
		[Display(Name = "暱稱")]
		public string? UserNickname { get; set; }
		public string? TradeCityKey { get; set; }
		public string? CityName { get; set; }
		public short TradeRegionId { get; set; }
		public string? RegionName { get; set; }
		[Display(Name = "LINE")]
		public string? ContactLine { get; set; }
		[Display(Name = "電話號碼")]
		public string? ContactPhone { get; set; }
		[Display(Name = "其他聯絡方式")]
		public string? ContactOther { get; set; }
	}

	//var info = from o in _context.CherishDefaultInfos
	//		   where o.GiverUserId == userId
	//		   select o;
	//ViewBag.DefaultInfo = info.Single();

	//		var city = from o in _context.Cities
	//				   select o;
	//ViewBag.CityList = city.ToList();
}
