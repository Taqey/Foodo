namespace Foodo.Application.Models.Dto.Order
{
	public class MerchantOrderDto : OrderBaseDto
	{
		public string CustomerId { get; set; }
		public string CustomerName { get; set; }
	}
}