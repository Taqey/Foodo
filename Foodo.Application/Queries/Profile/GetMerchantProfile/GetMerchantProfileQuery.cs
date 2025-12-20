using Foodo.Application.Models.Dto.Profile.Merchant;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Profile.GetMerchantProfile
{
	public class GetMerchantProfileQuery : IRequest<ApiResponse<MerchantProfileDto>>
	{
		public string UserId { get; set; }

	}
}
