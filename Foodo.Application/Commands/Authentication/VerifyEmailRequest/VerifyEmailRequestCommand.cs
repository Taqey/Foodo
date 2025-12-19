using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Authentication.VerifyEmailRequest
{
	public class VerifyEmailRequestCommand : IRequest<ApiResponse>
	{
		public string Email { get; set; }
		public string Role { get; set; }
	}
}
