using Dapper;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Domain.Enums;
using System.Data;
using System.Text;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Restaurants
{
	public class RestaurantsReadService : IRestaurantsReadService
	{
		private readonly IDbConnection _connection;

		public RestaurantsReadService(IDbConnection connection)
		{
			_connection = connection;
		}
		public async Task<PaginationDto<ShopDto>> ReadRestaurants(int Page, int PageSize, RestaurantCategory? Category)
		{
			var filterSql = new StringBuilder();
			var parameters = new DynamicParameters();

			if (Category.HasValue)
			{
				filterSql.Append(@"
									INNER JOIN TblRestaurantCategories rc
										ON m.UserId = rc.RestaurantId
									WHERE rc.CategoryId = @CategoryId
								");

				parameters.Add("CategoryId", Category.Value);
			}

			// =========================
			// Count Query
			// =========================
			var countSql = $@"
							SELECT COUNT(DISTINCT m.UserId)
							FROM TblMerchants m
							{filterSql}
						";

			var totalCount = await _connection.ExecuteScalarAsync<int>(countSql, parameters);

			var idsSql = $@"
							SELECT DISTINCT m.UserId
							FROM TblMerchants m
							{filterSql}
							ORDER BY m.UserId
							OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY
						";

			parameters.Add("Offset", (Page - 1) * PageSize);
			parameters.Add("Fetch", PageSize);

			var ids = (await _connection.QueryAsync<string>(idsSql, parameters)).ToList();

			if (totalCount == 0)
			{
				return new PaginationDto<ShopDto>
				{
					Items = new List<ShopDto>(),
					TotalItems = 0,
					TotalPages = 0,

				};
			}

			// =========================
			// Data Query
			// =========================
			var dataSql = @"
        SELECT 
* from vw_restaurants
        WHERE UserId IN @ids
    ";

			var raw = await _connection.QueryAsync<ShopRawDto>(dataSql, new { ids });

			var result = raw
				.GroupBy(o => o.UserId)
				.Select(e => new ShopDto
				{
					ShopId = e.Key,
					ShopName = e.First().StoreName,
					ShopDescription = e.First().StoreDescription,
					url = e.FirstOrDefault(x => x.Url != null)?.Url,
					Categories = e
						.Where(x => x.categoryid.HasValue)
						.Select(x => ((RestaurantCategory)x.categoryid.Value).ToString())
						.Distinct()
						.ToList()
				})
				.ToList();

			return new PaginationDto<ShopDto>
			{
				Items = result,
				TotalItems = totalCount,
				TotalPages = (int)(Math.Ceiling(totalCount / (double)PageSize)),

			};
		}
	}
}
