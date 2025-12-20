using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Addresses.DeleteAddress
{
	public class DeleteAddressCommand : IRequest<ApiResponse>
	{
		public string userId { get; set; }
		public int adressId { get; set; }
	}
}
