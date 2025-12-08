namespace Foodo.Application.Models.Input.Merchant
{
	public class ProductUpdateInput
	{
		public int productId { get; set; }
		public string ProductName { get; set; }
		public string ProductDescription { get; set; }
		public string Price { get; set; }
	}
}