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
			string query = @"SELECT 
							o.OrderId,
							o.OrderDate,
							o.TotalPrice,
							o.OrderStatus,
							c.UserId      AS CustomerId,
							CONCAT(c.FirstName,' ',c.LastName)   AS CustomerName,
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
