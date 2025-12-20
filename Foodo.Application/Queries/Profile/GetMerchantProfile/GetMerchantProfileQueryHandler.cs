using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Profile.MerchantProfile;
using Foodo.Application.Models.Dto.Profile.Merchant;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Profile.GetMerchantProfile
{
	public class GetMerchantProfileQueryHandler : IRequestHandler<GetMerchantProfileQuery, ApiResponse<MerchantProfileDto>>
	{
		private readonly IMerchantProfileReadService _service;

		public GetMerchantProfileQueryHandler(IMerchantProfileReadService service)
		{
			_service = service;
		}
		public async Task<ApiResponse<MerchantProfileDto>> Handle(GetMerchantProfileQuery request, CancellationToken cancellationToken)
		{
			var result = await _service.ReadMerchantProfile(request.UserId);
			return ApiResponse<MerchantProfileDto>.Success(result, "Data retrieved successfully");
		}
	}
}
