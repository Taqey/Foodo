public record CustomerOrderRawDto
{
	public int OrderId { get; set; }
	public DateTime OrderDate { get; set; }
	public decimal TotalPrice { get; set; }
	public string OrderStatus { get; set; }
	public string MerchantId { get; set; }
	public string MerchantName { get; set; }
	public string BillingAddress { get; set; }
	public int ProductId { get; set; }
	public string ProductsName { get; set; }
	public int Quantity { get; set; }
	public decimal Price { get; set; }
}
