using Foodo.Application.Models.Response;
using Foodo.Domain.Enums;
using MediatR;

namespace Foodo.Application.Commands.Authentication.AddCategory
{
	public class AddCategoryCommand : IRequest<ApiResponse>
	{
		public string UserId { get; set; }
		public List<RestaurantCategory> restaurantCategories { get; set; } = new List<RestaurantCategory>();
	}
}
