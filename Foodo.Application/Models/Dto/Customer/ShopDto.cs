namespace Foodo.Application.Models.Dto.Customer
{
	public class ShopDto
	{
		public string ShopId { get; set; }
		public string ShopName { get; set; }
		public string ShopDescription { get; set; }
		public List<string> Categories { get; set; } = new List<string>();
		public string url { get; set; }

	}
}