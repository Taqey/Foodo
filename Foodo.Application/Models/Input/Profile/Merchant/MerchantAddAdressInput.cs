using Foodo.Application.Models.Dto.Profile.Merchant;

namespace Foodo.Application.Models.Input.Profile.Merchant
{
	public class MerchantAddAdressInput
	{
		public string MerchantId { get; set; }
		public List<MerchantAddAdressDto> Adresses { get; set; } = new List<MerchantAddAdressDto>();
	}
}
