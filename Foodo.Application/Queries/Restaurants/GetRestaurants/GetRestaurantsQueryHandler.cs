using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Restaurants;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Restaurants.GetRestaurants
{
	public class GetRestaurantsQueryHandler : IRequestHandler<GetRestaurantsQuery, ApiResponse<PaginationDto<ShopDto>>>
	{
		private readonly IRestaurantsReadService _service;

		public GetRestaurantsQueryHandler(IRestaurantsReadService service)
		{
			_service = service;
		}
		public async Task<ApiResponse<PaginationDto<ShopDto>>> Handle(GetRestaurantsQuery request, CancellationToken cancellationToken)
		{
			var result = await _service.ReadRestaurants(request.Page, request.PageSize, request.Category);

			return ApiResponse<PaginationDto<ShopDto>>.Success(result);
		}
	}
}
