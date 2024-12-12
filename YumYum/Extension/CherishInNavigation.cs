using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using YumYum.Models;

namespace YumYum.Extension
{
	public static class CherishInNavigation
	{
		public static IQueryable<CherishOrder> MatchInNavigation(this IQueryable<CherishOrder> c)
		{
			return c.Include(c => c.IngredAttribute)
						.Include(c => c.Ingredient)
						.Include(c => c.GiverUser)
						.Include(c => c.CherishOrderInfo)
						.ThenInclude(c=>c!.TradeCityKeyNavigation)
						.Include(c => c.CherishOrderInfo)
						.ThenInclude(c => c!.TradeRegion)
						.Include(c => c.CherishOrderCheck);
		}

		public static IQueryable<CherishOrder> MatchInNavigationOptionalthis(IQueryable<CherishOrder> query,

		bool includeIngredient = false,

		bool includeAttribute = false,

		bool includeGiverUser = false,

		bool includeOrderInfo = false,

		bool includeOrderCheck = false)
		{
			if (includeIngredient)
				query = query.Include(c => c.Ingredient);

			if (includeAttribute)
				query = query.Include(c => c.IngredAttribute);

			if (includeGiverUser)
				query = query.Include(c => c.GiverUser);

			if (includeOrderInfo)
				query = query.Include(c => c.CherishOrderInfo)
							 .ThenInclude(ci => ci.TradeRegion);

			if (includeOrderCheck)
				query = query.Include(c => c.CherishOrderCheck);

			return query;
		}
	}
}
