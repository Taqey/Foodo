using Foodo.Domain.Enums;

namespace Foodo.API.Models.Request
{
	public class ShopCategoryPaginationRequest
	{
		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public RestaurantCategory Category { get; set; }

	}
}