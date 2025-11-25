using Foodo.Domain.Enums;

namespace Foodo.Application.Models.Input.Merchant
{
	public class ProductCategoryInput
	{
		public int ProductId { get; set; }
		public List<FoodCategory> restaurantCategories { get; set; }
	}
}