using Foodo.Application.Abstraction.InfrastructureRelatedServices.Authentication;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.User;
using Foodo.Application.Models.Dto.Auth;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Foodo.Application.Commands.Authentication.Login
{
	public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<JwtDto>>
	{
		private readonly IUserService _userService;
		private readonly ICreateToken _createToken;
		private readonly IHttpContextAccessor _http;

		public LoginCommandHandler(IUserService userService, ICreateToken createToken, IHttpContextAccessor http)
		{
			_userService = userService;
			_createToken = createToken;
			_http = http;
		}
		public async Task<ApiResponse<JwtDto>> Handle(LoginCommand input, CancellationToken cancellationToken)
		{
			ApplicationUser? user;
			if (input.Email.Contains('@'))
			{
				user = await _userService.GetByEmailAsync(input.Email);
			}
			else
			{
				user = await _userService.GetByUsernameAsync(input.Email);
			}
			if (user == null)
			{
				return ApiResponse<JwtDto>.Failure("Invalid email or username.");
			}
			var isPasswordValid = await _userService.CheckPasswordAsync(user, input.Password);
			if (!isPasswordValid)
			{
				return ApiResponse<JwtDto>.Failure("Invalid password.");
			}
			var result = await CreateTokens(user, (await _userService.GetRolesForUser(user)).FirstOrDefault());



			return ApiResponse<JwtDto>.Success(result, "Login successful.");
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
