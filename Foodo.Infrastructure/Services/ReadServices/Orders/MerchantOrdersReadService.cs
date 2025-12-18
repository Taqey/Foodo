using Dapper;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Orders;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Order;
using System.Data;

namespace Foodo.Infrastructure.Services.ReadServices.Order
{
	public class MerchantOrdersReadService : IMerchantOrdersReadService
	{
		private readonly IDbConnection _connection;

		public MerchantOrdersReadService(IDbConnection connection)
		{
			_connection = connection;
		}
		public async Task<PaginationDto<MerchantOrderDto>> GetMerchantOrders(string merchantId, int page, int pageSize)
		{
			string sqlOrderIds = @"	SELECT OrderId
									FROM TblOrders
									WHERE MerchantId = @MerchantId
									ORDER BY OrderDate DESC
									OFFSET @Offset ROWS 
									FETCH NEXT @PageSize ROWS ONLY;";

			var orderIds = (await _connection.QueryAsync<int>(sqlOrderIds, new
			{
				MerchantId = merchantId,
				Offset = (page - 1) * pageSize,
				PageSize = pageSize
			})).ToList();

			if (!orderIds.Any())
				return new PaginationDto<MerchantOrderDto>
				{
					TotalItems = 0,
					TotalPages = 0,
					Items = new List<MerchantOrderDto>()
				};

			string sqlOrdersDetails = @"	SELECT 
											o.OrderId,
											o.OrderDate,
											o.TotalPrice,
											o.OrderStatus,
											c.UserId      AS CustomerId,
											CONCAT(c.FirstName,' ',c.LastName) AS CustomerName,
											p.ProductId,
											p.ProductsName,
											po.Quantity,
											po.Price
											FROM TblOrders o
											JOIN TblAdresses a ON a.AddressId = o.BillingAddressId
											JOIN TblProductsOrders po ON po.OrderId = o.OrderId
											JOIN TblProducts p ON po.ProductId = p.ProductId
											JOIN TblMerchants m ON p.UserId = m.UserId
											JOIN TblCustomers c ON o.CustomerId = c.UserId
											WHERE o.OrderId IN @OrderIds
											ORDER BY o.OrderDate DESC;";

			var rawOrders = await _connection.QueryAsync<MerchantOrderRawDto>(
				sqlOrdersDetails, new { OrderIds = orderIds });

			var orders = rawOrders
				.GroupBy(o => o.OrderId)
				.Select(g => new MerchantOrderDto
				{
					CustomerId = g.First().CustomerId,
					OrderId = g.First().OrderId,
					CustomerName = g.First().CustomerName,
					OrderDate = g.First().OrderDate,
					Status = g.First().OrderStatus,
					TotalAmount = g.First().TotalPrice,
					OrderItems = g.Select(o => new OrderItemDto
					{
						ItemId = o.ProductId,
						ItemName = o.ProductsName,
						Price = o.Price,
						Quantity = o.Quantity
					}).ToList()
				}).ToList();

			string countSql = @"SELECT COUNT(DISTINCT o.OrderId)
                        FROM TblOrders o
                        WHERE o.MerchantId = @MerchantId;";
			var totalItems = await _connection.ExecuteScalarAsync<int>(countSql, new { MerchantId = merchantId });
			var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

			return new PaginationDto<MerchantOrderDto>
			{
				TotalItems = totalItems,
				TotalPages = totalPages,
				Items = orders
			};
		}
	}
}
