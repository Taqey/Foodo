using Foodo.Application.Queries.Orders.GetOrders.GetCustomerOrder;
using Foodo.Application.Queries.Orders.GetOrders.GetCustomerOrders;
using Foodo.Application.Queries.Orders.GetOrders.GetMerchantOrder;
using Foodo.Application.Queries.Orders.GetOrders.GetMerchantOrders;
using Foodo.Application.Queries.Orders.GetOrders.GetOrder;
using Foodo.Application.Queries.Orders.GetOrders.GetOrders;
using Foodo.Domain.Enums;
using System.Security.Claims;

namespace Foodo.Application.Factory.Order
{
	public class OrderStrategyFactory : IOrderStrategyFactory
	{

		public GetOrdersQuery GetOrdersStrategy(ClaimsPrincipal user, int page, int pageSize)
		{
			var role = user.FindFirst(ClaimTypes.Role)?.Value;
			var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			switch (role)
			{
				case nameof(UserType.Merchant):
					return new GetMerchantOrdersQuery { Page = page, PageSize = pageSize, UserId = userId };
				case nameof(UserType.Customer):
					return new GetCustomerOrdersQuery { Page = page, PageSize = pageSize, UserId = userId };
				default:
					throw new ArgumentException("Invalid role");
			}
		}

		public GetOrderQuery GetOrderStrategy(ClaimsPrincipal user, int orderId)
		{
			var role = user.FindFirst(ClaimTypes.Role)?.Value;
			var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			switch (role)
			{
				case nameof(UserType.Merchant):
					return new GetMerchantOrderQuery { OrderId = orderId };
				case nameof(UserType.Customer):
					return new GetCustomerOrderQuery { OrderId = orderId };
				default:
					throw new ArgumentException("Invalid role");
			}
		}
	}
}
