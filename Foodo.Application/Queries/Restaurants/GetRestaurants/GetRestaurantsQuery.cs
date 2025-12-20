using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Application.Models.Response;
using Foodo.Domain.Enums;
using MediatR;

namespace Foodo.Application.Queries.Restaurants.GetRestaurants;

public class GetRestaurantsQuery : IRequest<ApiResponse<PaginationDto<ShopDto>>>
{
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 10;
	public RestaurantCategory? Category { get; set; }
}
