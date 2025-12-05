using Foodo.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Foodo.API.Models.Request.Customer
{
	public class ShopCategoryPaginationRequest
	{
		[Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than 0")]
		public int PageNumber { get; set; } = 1;

		[Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
		public int PageSize { get; set; } = 10;
		[Required(ErrorMessage = "Category is required")]
		[EnumDataType(typeof(RestaurantCategory), ErrorMessage = "Invalid category value")]
		public RestaurantCategory Category { get; set; }

	}
}