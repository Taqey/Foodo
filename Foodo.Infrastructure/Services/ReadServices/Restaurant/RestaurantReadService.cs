using Dapper;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Domain.Enums;
using System.Data;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Restaurant
{
	public class RestaurantReadService : IRestaurantReadService
	{
		private readonly IDbConnection _connection;

		public RestaurantReadService(IDbConnection connection)
		{
			_connection = connection;
		}
		public async Task<ShopDto> ReadRestaurant(string RestaurantId)
		{
			var sql = @"SELECT * from
vw_restaurant
                  where UserId=@Id";
			var raw = await _connection.QueryAsync<ShopRawDto>(sql, new { Id = RestaurantId });
			var result = raw.GroupBy(o => o.UserId).Select(e => new ShopDto
			{
				ShopId = e.Key,
				ShopName = e.First().StoreName,
				ShopDescription = e.First().StoreDescription,
				url = e.First().Url,
				Categories = e
						.Where(m => m.categoryid.HasValue)
						.Select(x => ((RestaurantCategory)x.categoryid.Value).ToString())
						.Distinct()
						.ToList()
			}).FirstOrDefault();
			return result;
		}
	}
}
