using Foodo.Application.Models.Dto.Profile.Merchant;

namespace Foodo.API.Models.Request.Profile.Merchant
{
	public class MerchantAddAdressRequest
	{
		public List<MerchantAddAdressDto> Adresses { get; set; } = new List<MerchantAddAdressDto>();

	}
}
