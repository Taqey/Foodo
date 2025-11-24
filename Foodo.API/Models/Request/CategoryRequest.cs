using Foodo.Domain.Enums;

namespace Foodo.API.Models.Request
{
	public class CategoryRequest
	{
		public List<FoodCategory> Categories { get; set; } = new List<FoodCategory>();
	}
}