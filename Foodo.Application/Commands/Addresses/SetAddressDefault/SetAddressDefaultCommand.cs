using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Addresses.SetAddressDefault
{
	public class SetAddressDefaultCommand : IRequest<ApiResponse>
	{
		public string CustomerId { get; set; }
		public int AdressId { get; set; }
	}
}
