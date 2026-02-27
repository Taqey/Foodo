using Dapper;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Order;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Order;
using System.Data;

namespace Foodo.Infrastructure.Services.ReadServices.Order
{
	public class CustomerOrderReadService : ICustomerOrderReadService
	{
		private readonly IDbConnection _connection;

		public CustomerOrderReadService(IDbConnection connection)
		{
			_connection = connection;
		}
		public async Task<CustomerOrderDto> GetCustomerOrder(int OrderId)
		{
			string query = @"select* from vw_CustomerOrder
							WHERE OrderId = @OrderId";
			var rawOrder = await _connection.QueryAsync<CustomerOrderRawDto>(query, new { OrderId = OrderId });
			if (!rawOrder.Any())
			{
				return null;
			}

			var result = rawOrder.GroupBy(o => o.OrderId).Select(e => new CustomerOrderDto
			{
				OrderId = e.First().OrderId,
				BillingAddress = e.First().BillingAddress,
				MerchantId = e.First().MerchantId,
				MerchantName = e.First().MerchantName,
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
