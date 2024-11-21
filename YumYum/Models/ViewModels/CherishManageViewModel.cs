namespace YumYum.Models.ViewModels
{
	public class CherishManageViewModel
	{
		// [訂單類]
		// 訂單編號
		public int CherishId { get; set; }

		// 訂單狀態碼
		public byte TradeStateCode { get; set; }

		// 使用者編號(食材提供者)
		public int GiverUserId { get; set; }

		// 數量
		public short Quantity { get; set; }

		// 截止日期
		public DateOnly EndDate { get; set; }

		// 食材來源
		public string? ObtainSource { get; set; }

		// 購買日期
		public DateOnly ObtainDate { get; set; }

		// 有效期限
		public DateOnly? CherishValidDate { get; set; }

		// 圖片路徑
		public string? CherishPhoto { get; set; }

		// [食材類]
		// 食材名稱
		public string? IngredientName { get; set; }
		// 食材分類
		public string? IngredAttributeName { get; set; }

		// [地區類]
		// 縣市
		public string? CityName { get; set; }
		// 鄉鎮市區
		public string? RegionName { get; set; }

		// [聯絡類]
		// 暱稱
		public string? UserNickname { get; set; }
		// 聯絡方式
		public string? ContactLine { get; set; }
		public string? ContactPhone { get; set; }
		public string? ContactOther { get; set; }


		// [審查狀態]
		// 請修改的原因
		public string? ReasonText { get; set; }

		// 退回原因
		public string? RejectText { get; set; }
	}
}
