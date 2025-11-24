namespace Foodo.API.Controllers
{
	public class ProductPaginationByShopRequest
	{

		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public string MerchantId { get; set; }
	}
}