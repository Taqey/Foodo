using Dapper;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Order;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Order;
using System.Data;

namespace Foodo.Infrastructure.Services.ReadServices.Order
{
	public class MerchantOrderReadService : IMerchantOrderReadService
	{
		private readonly IDbConnection _connection;

		public MerchantOrderReadService(IDbConnection connection)
		{
			_connection = connection;
		}
		public async Task<MerchantOrderDto> GetMerchantOrder(int OrderId)
		{
			string query = @"select* from vw_MerchantOrder where OrderId= @OrderId";
			var rawOrder = await _connection.QueryAsync<MerchantOrderRawDto>(query, new { OrderId = OrderId });

			var result = rawOrder.GroupBy(o => o.OrderId).Select(e => new MerchantOrderDto
			{
				OrderId = e.First().OrderId,
				CustomerId = e.First().CustomerId,
				CustomerName = e.First().CustomerName,
				OrderDate = e.First().OrderDate,
				Status = e.First().OrderStatus,
				TotalAmount = e.First().TotalPrice,
				OrderItems = e.Select(o => new OrderItemDto
				{
					ItemId = o.ProductId,
					ItemName = o.ProductsName,
					Price = o.Price,
					Quantity = o.Quantity
				}).ToList()
			}).FirstOrDefault();

			return result;
		}
	}
}
