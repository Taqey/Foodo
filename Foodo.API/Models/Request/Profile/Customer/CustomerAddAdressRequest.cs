using Foodo.Application.Models.Dto.Profile.Customer;
using Foodo.Application.Models.Dto.Profile.Merchant;

namespace Foodo.API.Models.Request.Profile.Customer
{
	public class CustomerAddAdressRequest
	{
		public List<CustomerAddAdressDto> Adresses { get; set; } = new List<CustomerAddAdressDto>();

	}
}