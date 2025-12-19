using Foodo.Application.Models.Dto.Auth;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Authentication.RefreshToken
{
	public class RefreshTokenCommand : IRequest<ApiResponse<JwtDto>>
	{
		public string Token { get; set; }
	}
}
