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
			string query = @"SELECT 
							o.OrderId,
							o.OrderDate,
							o.TotalPrice,
							o.OrderStatus,
							o.MerchantId  AS MerchantId,
							m.StoreName   AS MerchantName,
							a.StreetAddress AS BillingAddress,
							p.ProductId,
							p.ProductsName,
							po.Quantity,
							po.Price
							FROM TblOrders o
							JOIN TblAdresses a ON a.AddressId = o.BillingAddressId
							JOIN TblProductsOrders po ON po.OrderId = o.OrderId
							JOIN TblProducts p ON po.ProductId = p.ProductId
							JOIN TblMerchants m ON p.UserId = m.UserId
							join TblCustomers c on o.CustomerId=c.UserId
							WHERE o.OrderId = @OrderId";
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
