using Foodo.Application.Abstraction.Authentication;
using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Models.Dto.Auth;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Foodo.Application.Commands.Authentication.RefreshToken
{
	public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<JwtDto>>
	{
		private readonly IUserService _userService;
		private readonly ICreateToken _createToken;
		private readonly IHttpContextAccessor _http;

		public RefreshTokenCommandHandler(IUserService userService, ICreateToken createToken, IHttpContextAccessor http)
		{
			_userService = userService;
			_createToken = createToken;
			_http = http;
		}
		public async Task<ApiResponse<JwtDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
		{
			var user = await _userService.GetUserByRefreshToken(request.Token);
			if (user == null)
			{
				return ApiResponse<JwtDto>.Failure("Invalid refresh token.");
			}
			var token = user.lkpRefreshTokens.FirstOrDefault(e => e.Token == request.Token);
			if (token == null)
			{
				return ApiResponse<JwtDto>.Failure("Token not found");
			}
			if (!token.IsActive)
			{
				return ApiResponse<JwtDto>.Failure("Inactive or expired token");
			}
			token.RevokedOn = DateTime.UtcNow;
			var result = await CreateTokens(user, (await _userService.GetRolesForUser(user)).FirstOrDefault());
			return ApiResponse<JwtDto>.Success(result, "Token refreshed successfully.");
		}
		public async Task<JwtDto> CreateTokens(ApplicationUser user, string role)
		{
			var token = _createToken.CreateJwtToken(user, role);
			var RefreshToken = _createToken.CreatRefreshToken();
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Expires = RefreshToken.ExpiresOn.ToUniversalTime(),
				SameSite = SameSiteMode.None,
				Secure = true,
				Path = "/"
			};

			_http.HttpContext.Response.Cookies.Append("RefreshToken", RefreshToken.Token, cookieOptions);
			user.lkpRefreshTokens.Add(new lkpRefreshToken { Token = RefreshToken.Token, CreatedAt = RefreshToken.CreatedOn, ExpiresAt = RefreshToken.ExpiresOn });
			await _userService.UpdateAsync(user);
			return new JwtDto { Token = token.Token, CreatedOn = token.CreatedOn, ExpiresOn = token.ExpiresOn };

		}
	}
}
