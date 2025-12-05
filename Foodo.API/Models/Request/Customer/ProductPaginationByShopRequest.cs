using System.ComponentModel.DataAnnotations;

namespace Foodo.API.Models.Request.Customer
{
	public class ProductPaginationByShopRequest
	{
		[Required(ErrorMessage = "PageNumber is required.")]
		[Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than 0.")]
		public int PageNumber { get; set; } = 1;

		[Required(ErrorMessage = "PageSize is required.")]
		[Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
		public int PageSize { get; set; } = 10;

		[Required(ErrorMessage = "MerchantId is required")]
		public string MerchantId { get; set; }
	}
}
