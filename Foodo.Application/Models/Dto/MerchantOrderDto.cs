namespace Foodo.Application.Models.Dto
{
	public class MerchantOrderDto
	{
		public int OrderId { get; set; }
		public string CustomerId { get; set; }
		public string CustomerName { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal? TotalAmount { get; set; }
		public string Status { get; set; }
		public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
	}
}