using Foodo.Application.Models.Dto.Profile.Customer;

namespace Foodo.API.Models.Request.Profile.Customer
{
	public class AddAdressRequest
	{
		public List<CustomerAddAdressDto> Adresses { get; set; } = new List<CustomerAddAdressDto>();

	}
}