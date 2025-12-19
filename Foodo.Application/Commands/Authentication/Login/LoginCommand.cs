using Foodo.Application.Models.Dto.Auth;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Authentication.Login
{
	public class LoginCommand : IRequest<ApiResponse<JwtDto>>
	{
		public string Email { get; set; }
		public string Password { get; set; }
	}
}
