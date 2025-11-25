using Foodo.Domain.Enums;

namespace Foodo.Application.Models.Input.Customer
{
	public class ShopsPaginationByCategoryInput
	{
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public RestaurantCategory Category { get; set; }
	}
}