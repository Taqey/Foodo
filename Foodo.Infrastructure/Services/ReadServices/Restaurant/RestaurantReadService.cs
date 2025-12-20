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
			var sql = @"SELECT TblRestaurantCategories.categoryid, LkpUserPhotos.Url, TblMerchants.StoreName, TblMerchants.StoreDescription, TblMerchants.UserId
FROM     AspNetUsers left JOIN
                  LkpUserPhotos ON AspNetUsers.Id = LkpUserPhotos.UserId right JOIN
                  TblMerchants ON AspNetUsers.Id = TblMerchants.UserId left JOIN
                  TblRestaurantCategories ON TblMerchants.UserId = TblRestaurantCategories.restaurantid
                  where TblMerchants.UserId=@Id";
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
