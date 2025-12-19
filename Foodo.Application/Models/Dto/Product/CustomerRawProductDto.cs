namespace Foodo.Application.Models.Dto.Product
{
	public class CustomerRawProductDto
	{
		public int ProductId { get; set; }
		public string ProductsName { get; set; }
		public string ProductDescription { get; set; }
		public decimal Price { get; set; }
		public string Url { get; set; }
		public bool IsMain { get; set; }
		public string CategoryName { get; set; }
		public string AttributeName { get; set; }
		public string AttributeValue { get; set; }
		public string MeasuringUnit { get; set; }
	}
}
