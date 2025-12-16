namespace Foodo.Application.Models.Dto.Order
{
	public class CustomerOrderDto : OrderBaseDto
	{
		public string MerchantId { get; set; }
		public string MerchantName { get; set; }
		public string BillingAddress { get; set; }
	}
}
