using Foodo.Application.Models.Response;
using Foodo.Domain.Enums;
using MediatR;

namespace Foodo.Application.Commands.Products.RemoveProductCategory
{
	public class RemoveProductCategoryCommand : IRequest<ApiResponse>
	{
		public int ProductId { get; set; }
		public List<FoodCategory> restaurantCategories { get; set; }
	}
}
