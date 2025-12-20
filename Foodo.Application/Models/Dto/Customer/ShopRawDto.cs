namespace Foodo.Application.Models.Dto.Customer
{
	public class ShopRawDto
	{
		public string UserId { get; set; }
		public string StoreName { get; set; }
		public string StoreDescription { get; set; }
		public int? categoryid { get; set; }
		public string? Url { get; set; }
	}
}
