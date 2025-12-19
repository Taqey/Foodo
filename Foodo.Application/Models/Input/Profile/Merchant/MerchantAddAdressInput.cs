using Foodo.Application.Models.Dto.Profile.Customer;

namespace Foodo.Application.Models.Input.Profile.Merchant
{
	public class MerchantAddAdressInput
	{
		public string MerchantId { get; set; }
		public List<AddAdressDto> Adresses { get; set; } = new List<AddAdressDto>();
	}
}
