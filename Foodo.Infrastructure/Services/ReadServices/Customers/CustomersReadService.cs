using Dapper;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;
using System.Data;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Customers
{
	public class CustomersReadService : ICustomersReadService
	{
		private readonly IDbConnection _connection;

		public CustomersReadService(IDbConnection connection)
		{
			_connection = connection;
		}
		public async Task<PaginationDto<CustomerDto>> ReadCustomers(string restaurantId, int page, int pageSize)
		{
			var countSql = @"
select CompletedCustomerCount from vw_NumberOfCompletedCustomers where MerchantId= @MerchantId;
						";
			var totalItems = await _connection.ExecuteScalarAsync<int>(countSql, new { MerchantId = restaurantId });
			if (totalItems == 0)
			{
				return new PaginationDto<CustomerDto>
				{
					Items = new List<CustomerDto>(),
					TotalItems = 0,
					TotalPages = 0,

				};
			}
			var sql = @"select FullName,Email,PhoneNumber,TotalSpent,TotalOrders,LastPurchased from vw_CompletedCustomers where MerchantId = @MerchantId			
ORDER BY TotalSpent DESC  
						OFFSET @OFFSET ROWS 
						FETCH NEXT @FETCH ROWS ONLY;

						";
			var result = (await _connection.QueryAsync<CustomerDto>(sql, new { MerchantId = restaurantId, OFFSET = (page - 1) * pageSize, FETCH = pageSize })).ToList();
			return new PaginationDto<CustomerDto>
			{
				Items = result,
				TotalItems = totalItems,
				TotalPages = (int)(Math.Ceiling(totalItems / (double)pageSize)),
			};
		}
	}
}
