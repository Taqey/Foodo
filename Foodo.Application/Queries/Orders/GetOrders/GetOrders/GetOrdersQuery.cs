using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Order;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Orders.GetOrders.GetOrders
{
	public class GetOrdersQuery : IRequest<ApiResponse<PaginationDto<OrderBaseDto>>>
	{
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public string UserId { get; set; }
	}
}
