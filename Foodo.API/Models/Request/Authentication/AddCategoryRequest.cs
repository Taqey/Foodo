using Foodo.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Foodo.API.Models.Request.Authentication
{
	public class AddCategoryRequest
	{
		[Required]
		public string UserId { get; set; }

		[Required]
		[MinLength(1, ErrorMessage = "At least one category is required")]
		public List<RestaurantCategory> RestaurantCategories { get; set; }
	}

}