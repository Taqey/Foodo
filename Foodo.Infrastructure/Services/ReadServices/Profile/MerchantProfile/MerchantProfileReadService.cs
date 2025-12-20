using Dapper;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Profile.MerchantProfile;
using Foodo.Application.Models.Dto.Profile.Merchant;
using Foodo.Domain.Enums;
using System.Data;

namespace Foodo.Infrastructure.Services.ReadServices.Profile.MerchantProfile
{
	public class MerchantProfileReadService : IMerchantProfileReadService
	{
		private readonly IDbConnection _connection;

		public MerchantProfileReadService(IDbConnection connection)
		{
			_connection = connection;
		}
		public async Task<MerchantProfileDto> ReadMerchantProfile(string UserId)
		{
			var sql = @"
						SELECT 
							A.City,
							A.State,
							A.StreetAddress,
							A.PostalCode,
							A.Country,
							U.Id,
							M.StoreName,
							M.StoreDescription,
							RC.CategoryId,
							U.PhoneNumber,
							U.EmailConfirmed,
							U.Email,
							A.AddressId
						FROM AspNetUsers U
						left JOIN TblAdresses A 
							ON U.Id = A.UserId
						INNER JOIN TblMerchants M 
							ON U.Id = M.UserId
						LEFT JOIN TblRestaurantCategories RC 
							ON M.UserId = RC.RestaurantId
						where U.Id=@Id
						";
			var raw = await _connection.QueryAsync<MerchantProfileRawDto>(sql, new { Id = UserId });
			var result = raw.GroupBy(o => o.Id).Select(e => new MerchantProfileDto
			{
				StoreName = e.First().StoreName,
				StoreDescription = e.First().StoreDescription,
				Email = e.First().Email,
				IsEmailConfirmed = e.First().EmailConfirmed,
				categories = e
							.Where(x => x.CategoryId.HasValue)
							.Select(x => ((RestaurantCategory)x.CategoryId.Value).ToString())
							.Distinct()
							.ToList(),
				Adresses = e.GroupBy(m => m.AddressId).Select(o => new MerchantAdressDto { Id = o.First().AddressId, City = o.First().City, Country = o.First().Country, StreetAddress = o.First().StreetAddress, PostalCode = o.First().PostalCode, State = o.First().State }).ToList(),
			}).FirstOrDefault();
			return result;
		}
	}
}
