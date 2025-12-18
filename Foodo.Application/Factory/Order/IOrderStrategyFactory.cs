using Foodo.Application.Queries.Orders.GetOrder.GetOrder;
using Foodo.Application.Queries.Orders.GetOrders.GetOrders;
using System.Security.Claims;

namespace Foodo.Application.Factory.Order
{
	public interface IOrderStrategyFactory
	{
		GetOrdersQuery GetOrdersStrategy(ClaimsPrincipal user, int page, int pageSize);
		GetOrderQuery GetOrderStrategy(ClaimsPrincipal user, int orderId);

	}

}