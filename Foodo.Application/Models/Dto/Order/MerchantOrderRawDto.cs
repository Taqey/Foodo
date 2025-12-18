namespace Foodo.Application.Models.Dto.Order
{
	public record MerchantOrderRawDto
	{
		public int OrderId { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal TotalPrice { get; set; }
		public string OrderStatus { get; set; }
		public string CustomerId { get; set; }
		public string CustomerName { get; set; }
		public int ProductId { get; set; }
		public string ProductsName { get; set; }
		public int Quantity { get; set; }
		public decimal Price { get; set; }
	}
}
