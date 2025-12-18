using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Orders.CancelOrder
{
	public record CancelOrderCommand : IRequest<ApiResponse>
	{
		public int OrderId { get; set; }
	}
}
