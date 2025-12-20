using Dapper;
using Foodo.Application.Models.Dto.Profile.Customer;
using System.Data;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Profile.CustomerProfile
{
	public class CustomerProfileReadService : ICustomerProfileReadService
	{
		private readonly IDbConnection _connection;

		public CustomerProfileReadService(IDbConnection connection)
		{
			_connection = connection;
		}
		public async Task<CustomerProfileDto> ReadCustomerProfile(string UserId)
		{
			var sql = @"SELECT TblAdresses.City, TblAdresses.State, TblAdresses.StreetAddress, TblAdresses.PostalCode, TblAdresses.Country, TblAdresses.IsDefault, AspNetUsers.Id, TblCustomers.FirstName, TblCustomers.LastName, TblCustomers.Gender, 
                  TblCustomers.BirthDate, AspNetUsers.PhoneNumber, AspNetUsers.EmailConfirmed, AspNetUsers.Email,TblAdresses.AddressId
				FROM     AspNetUsers INNER JOIN
                  TblAdresses ON AspNetUsers.Id = TblAdresses.UserId INNER JOIN
                  TblCustomers ON AspNetUsers.Id = TblCustomers.UserId
                  where AspNetUsers.Id=@Id";
			var raw = await _connection.QueryAsync<CustomerProfileRawDto>(sql, new { Id = UserId });
			var result = raw.GroupBy(o => o.Id).Select(o => new CustomerProfileDto
			{
				FirstName = o.First().FirstName,
				LastName = o.First().LastName,
				Gender = o.First().Gender,
				BirthDate = DateOnly.FromDateTime(o.First().BirthDate),
				Email = o.First().Email,
				PhoneNumber = o.First().PhoneNumber,
				IsEmailConfirmed = o.First().EmailConfirmed,
				Adresses = o.Select(e => new CustomerAdressDto { Id = e.AddressId, City = e.City, Country = e.Country, StreetAddress = e.StreetAddress, PostalCode = e.PostalCode, IsDefault = e.IsDefault, State = e.State }).ToList()

			}).FirstOrDefault();
			return result;
		}
	}
}
