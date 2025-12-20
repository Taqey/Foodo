using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Restaurant;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Restaurants.GetRestaurant
{
	public class GetRestaurantQueryHandler : IRequestHandler<GetRestaurantQuery, ApiResponse<ShopDto>>
	{
		private readonly IRestaurantReadService _service;

		public GetRestaurantQueryHandler(IRestaurantReadService service)
		{
			_service = service;
		}
		public async Task<ApiResponse<ShopDto>> Handle(GetRestaurantQuery request, CancellationToken cancellationToken)
		{
			var result = await _service.ReadRestaurant(request.RestaurantId);
			return ApiResponse<ShopDto>.Success(result);
		}
	}
}
