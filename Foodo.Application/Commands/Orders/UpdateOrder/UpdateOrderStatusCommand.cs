using Foodo.Application.Models.Response;
using Foodo.Domain.Enums;
using MediatR;

namespace Foodo.Application.Commands.Orders.UpdateOrder
{
	public record UpdateOrderStatusCommand : IRequest<ApiResponse>
	{
		public OrderState OrderState { get; set; }
		public int OrderId { get; set; }

	}
}
