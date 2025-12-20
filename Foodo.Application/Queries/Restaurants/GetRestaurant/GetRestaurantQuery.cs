using Foodo.Application.Models.Dto.Customer;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Restaurants.GetRestaurant
{
	public class GetRestaurantQuery : IRequest<ApiResponse<ShopDto>>
	{
		public string RestaurantId { get; set; }
	}
}
