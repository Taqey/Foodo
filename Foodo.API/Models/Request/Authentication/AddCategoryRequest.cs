using Foodo.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Foodo.API.Models.Request.Authentication
{
	public class AddCategoryRequest
	{
		[Required]
		public string restaurantCategories { get; set; }
		public string UserId { get; set; }
	}
}