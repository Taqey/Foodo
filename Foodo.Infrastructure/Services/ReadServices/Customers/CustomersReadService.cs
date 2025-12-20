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
							SELECT COUNT(DISTINCT o.CustomerId) 
							FROM TblOrders o
							WHERE o.OrderStatus = 'Completed' 
							  AND o.MerchantId = @MerchantId;
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
			var sql = @"SELECT 
							CONCAT(c.FirstName, ' ', c.LastName) AS FullName,
							u.Email,
							u.PhoneNumber,
							SUM(o.TotalPrice) AS TotalSpent,
							COUNT(o.OrderId) AS TotalOrders,
							MAX(o.OrderDate) AS LastPurchased
						FROM AspNetUsers u
						INNER JOIN TblCustomers c 
							ON u.Id = c.UserId
						INNER JOIN TblOrders o
							ON o.CustomerId = c.UserId
						WHERE o.OrderStatus = 'Completed' 
						  AND o.MerchantId = @MerchantId
						GROUP BY 
							c.FirstName,
							c.LastName,
							u.Email,
							u.PhoneNumber
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
