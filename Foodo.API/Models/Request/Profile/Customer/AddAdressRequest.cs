using Foodo.Application.Models.Dto.Profile.Customer;

namespace Foodo.API.Models.Request.Profile.Customer
{
	public class AddAdressRequest
	{
		public List<AddAdressDto> Adresses { get; set; } = new List<AddAdressDto>();

	}
}