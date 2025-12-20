using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Domain.Enums;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Restaurants
{
	public interface IRestaurantsReadService
	{
		Task<PaginationDto<ShopDto>> ReadRestaurants(int Page, int PageSize, RestaurantCategory? Category);
	}
}
