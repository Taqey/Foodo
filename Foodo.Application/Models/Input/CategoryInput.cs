using Foodo.Domain.Enums;

namespace Foodo.Application.Models.Input
{
	public class CategoryInput
	{
		public string UserId { get; set; }
		public List<RestaurantCategory> restaurantCategories { get; set; } = new List<RestaurantCategory>();

	}
}