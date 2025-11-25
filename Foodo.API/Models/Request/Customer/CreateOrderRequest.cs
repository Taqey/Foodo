using Foodo.Application.Models.Dto;

namespace Foodo.API.Models.Request.Customer
{
	public class CreateOrderRequest
	{
		public List<OrderItemDto> Items { get; set; }
		//public string DeliveryAddress { get; set; }
	}
}