using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Profile.CustomerProfile;
using Foodo.Application.Models.Dto.Profile.Customer;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Profile.GetCustomerProfile
{
	public class GetCustomerProfileQueryHandler : IRequestHandler<GetCustomerProfileQuery, ApiResponse<CustomerProfileDto>>
	{
		private readonly ICustomerProfileReadService _service;

		public GetCustomerProfileQueryHandler(ICustomerProfileReadService service)
		{
			_service = service;
		}
		public async Task<ApiResponse<CustomerProfileDto>> Handle(GetCustomerProfileQuery request, CancellationToken cancellationToken)
		{
			var result = await _service.ReadCustomerProfile(request.UserId);
			return ApiResponse<CustomerProfileDto>.Success(result, "Data retrieved successfully");
		}
	}
}
