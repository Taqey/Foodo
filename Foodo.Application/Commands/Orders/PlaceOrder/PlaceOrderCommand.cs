using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Orders.PlaceOrder
{
	public record PlaceOrderCommand : IRequest<ApiResponse>
	{
		public string CustomerId { get; set; }
		public List<OrderItemDto> Items { get; set; }
	}
}
