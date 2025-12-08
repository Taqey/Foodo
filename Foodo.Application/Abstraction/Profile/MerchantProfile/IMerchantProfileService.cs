using Foodo.Application.Models.Dto.Profile.Merchant;
using Foodo.Application.Models.Input.Profile.Merchant;
using Foodo.Application.Models.Response;

namespace Foodo.Application.Abstraction.Profile.MerchantProfile
{
	public interface IMerchantProfileService
	{
		Task<ApiResponse<MerchantProfileDto>> GetMerchantProfile(MerchantProfileInput input);
		Task<ApiResponse> AddAdress(MerchantAddAdressInput input);
		Task<ApiResponse> RemoveAdress(MerchantRemoveAdressInput input);
	}
}
