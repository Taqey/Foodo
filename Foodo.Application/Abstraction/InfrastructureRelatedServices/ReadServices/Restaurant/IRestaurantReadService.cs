using Foodo.Application.Models.Dto.Customer;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Restaurant
{
	public interface IRestaurantReadService
	{
		Task<ShopDto> ReadRestaurant(string RestaurantId);
	}
}
