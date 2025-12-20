using Foodo.Application.Models.Dto.Profile.Customer;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Addresses.CreateAddress
{
	public class CreateAddressCommand : IRequest<ApiResponse>
	{
		public string UserId { get; set; }
		public List<AddAdressDto> Adresses { get; set; } = new List<AddAdressDto>();
	}
}
