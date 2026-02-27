using Dapper;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Orders;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Order;
using System.Data;

namespace Foodo.Infrastructure.Services.ReadServices.Order
{
	public class CustomerOrdersReadService : ICustomerOrdersReadService
	{
		private readonly IDbConnection _connection;

		public CustomerOrdersReadService(IDbConnection connection)
		{
			_connection = connection;
		}
		public async Task<PaginationDto<CustomerOrderDto>> GetCustomerOrders(string customerId, int page, int pageSize)
		{
			string sqlOrderIds = @"	SELECT OrderId
									FROM TblOrders
									WHERE CustomerId = @CustomerId
									ORDER BY OrderDate DESC
									OFFSET @Offset ROWS
									FETCH NEXT @PageSize ROWS ONLY;";

			var orderIds = (await _connection.QueryAsync<int>(sqlOrderIds, new
			{
				CustomerId = customerId,
				Offset = (page - 1) * pageSize,
				PageSize = pageSize
			})).ToList();

			if (!orderIds.Any())
				return new PaginationDto<CustomerOrderDto>
				{
					TotalItems = 0,
					TotalPages = 0,
					Items = new List<CustomerOrderDto>()
				};

			string sqlOrdersDetails = @"	SELECT 
* from vw_CustomerOrders
										WHERE OrderId IN @OrderIds
										ORDER BY OrderDate DESC;";

			var rawOrders = await _connection.QueryAsync<CustomerOrderRawDto>(
				sqlOrdersDetails, new { OrderIds = orderIds });

			var orders = rawOrders
				.GroupBy(o => o.OrderId)
				.Select(g =>
				{
					var first = g.First();
					var order = new CustomerOrderDto
					{
						OrderId = first.OrderId,
						OrderDate = first.OrderDate,
						TotalAmount = first.TotalPrice,
						Status = first.OrderStatus,
						MerchantId = first.MerchantId,
						MerchantName = first.MerchantName,
						BillingAddress = first.BillingAddress
					};
					order.OrderItems = g.Select(x => new OrderItemDto
					{
						ItemId = x.ProductId,
						ItemName = x.ProductsName,
						Quantity = x.Quantity,
						Price = x.Price
					}).ToList();

					return order;
				})
				.ToList();

			string countSql = @"SELECT COUNT(DISTINCT o.OrderId)
                        FROM TblOrders o
                        WHERE o.CustomerId = @CustomerId;";

			var totalItems = await _connection.ExecuteScalarAsync<int>(countSql, new { CustomerId = customerId });
			var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

			return new PaginationDto<CustomerOrderDto>
			{
				TotalItems = totalItems,
				TotalPages = totalPages,
				Items = orders
			};
		}
	}
}
