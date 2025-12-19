using Foodo.Application.Models.Dto.Profile.Customer;

namespace Foodo.Application.Models.Input.Profile.Customer
{
	public class CustomerAddAdressInput
	{
		public string CustomerId { get; set; }
		public List<AddAdressDto> Adresses { get; set; } = new List<AddAdressDto>();


	}
}
