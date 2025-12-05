using Foodo.Application.Models.Dto.Profile.Merchant;
using System.ComponentModel.DataAnnotations;

namespace Foodo.API.Models.Request.Profile.Merchant
{
	public class MerchantAddAdressRequest
	{
		[Required]
		public List<MerchantAddAdressDto> Adresses { get; set; } = new List<MerchantAddAdressDto>();

	}
}
