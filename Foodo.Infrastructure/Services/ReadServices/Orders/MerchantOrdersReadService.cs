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

			string sqlOrdersDetails = @"	select *from vw_MerchantOrders
											WHERE OrderId IN @OrderIds
											ORDER BY OrderDate DESC;";

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
