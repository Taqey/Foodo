using Foodo.Application.Models.Dto.Profile.Customer;

namespace Foodo.API.Models.Request.Profile.Customer
{
	public class CustomerAddAdressRequest
	{
		public List<CustomerAddAdressDto> Adresses { get; set; } = new List<CustomerAddAdressDto>();

	}
}