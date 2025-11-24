namespace Foodo.API.Models.Request
{
	public class ProductUpdateRequest
	{
		public string ProductName { get; set; }
		public string ProductDescription { get; set; }
		public string Price { get; set; }
	}
}