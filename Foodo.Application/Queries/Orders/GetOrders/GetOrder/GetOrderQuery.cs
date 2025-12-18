using Foodo.Application.Models.Dto.Order;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Orders.GetOrders.GetOrder
{
	public class GetOrderQuery : IRequest<ApiResponse<OrderBaseDto>>
	{
		public int OrderId { get; set; }
	}
}
