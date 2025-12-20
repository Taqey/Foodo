using Foodo.Application.Models.Dto.Profile.Merchant;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Profile.MerchantProfile
{
	public interface IMerchantProfileReadService
	{
		Task<MerchantProfileDto> ReadMerchantProfile(string UserId);

	}
}
