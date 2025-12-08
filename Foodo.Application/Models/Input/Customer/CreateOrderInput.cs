using Foodo.Application.Models.Dto;

namespace Foodo.Application.Models.Input.Customer
{
	public class CreateOrderInput
	{
		public string CustomerId { get; set; }
		public List<OrderItemDto> Items { get; set; }
		//public string DeliveryAddress { get; set; }
	}
}
