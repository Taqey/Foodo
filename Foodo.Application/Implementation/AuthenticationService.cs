using Foodo.Application.Abstraction;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace Foodo.Application.Implementation
{
	public class AuthenticationService : IAuthenticationService
	{
		private readonly IUserService _userService;
		private readonly ICreateToken _createToken;
		private readonly IHttpContextAccessor _http;

		public AuthenticationService(IUserService userService, ICreateToken createToken, IHttpContextAccessor http)
		{
			_userService = userService;
			_createToken = createToken;
			_http = http;
		}
		public async Task<ApiResponse> Register(RegisterInput input)
		{
			ApplicationUser? existingUser = null;
			existingUser = await _userService.GetByEmailAsync(input.Email);
			if (existingUser != null)
			{
				return ApiResponse.Failure("Email is already in use.");
			}
			existingUser = await _userService.GetByUsernameAsync(input.UserName);
			if (existingUser != null)
			{
				return ApiResponse.Failure("Username is already taken.");
			}

			var user = new ApplicationUser { Email = input.Email, UserName = input.UserName, PhoneNumber = input.PhoneNumber };
			string Role = "";
			if (input.UserType == UserType.Customer)
			{
				user.TblCustomer = new TblCustomer { FirstName = input.FirstName, LastName = input.LastName, Gender = input.Gender.ToString() };
				Role = "Customer";
			}
			else
			{
				user.TblMerchant = new TblMerchant { StoreName = input.StoreName, StoreDescription = input.StoreDescription };
				Role = "Merchant";
			}
			IdentityResult result;
			result = await _userService.CreateUserAsync(user, input.Password);
			if (result.Succeeded)
			{
				result = await _userService.AddRolesToUser(user, Role);
				return ApiResponse.Success("User registered successfully.");
			}
			return ApiResponse.Failure("User registration failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
		}
		public async Task<ApiResponse<JwtDto>> Login(LoginInput input)
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

		public Task ChangePassword(ChangePasswordInput input)
		{
			throw new NotImplementedException();
		}

		public Task ForgetPassword(ForgetPasswordInput input)
		{
			throw new NotImplementedException();
		}

		public async Task<ApiResponse<JwtDto>> RefreshToken(string Token)
		{
			var user = await _userService.GetUserByToken(Token);
			if (user == null)
			{
				return ApiResponse<JwtDto>.Failure("Invalid refresh token.");
			}
			var token = user.lkpRefreshTokens.FirstOrDefault(e => e.Token == Token);
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
				Secure = true
			};
			_http.HttpContext.Response.Cookies.Append("RefreshToken", RefreshToken.Token, cookieOptions);
			user.lkpRefreshTokens.Add(new lkpRefreshToken { Token = RefreshToken.Token, CreatedAt = RefreshToken.CreatedOn, ExpiresAt = RefreshToken.ExpiresOn });
			await _userService.UpdateAsync(user);
			return new JwtDto { Token = token.Token, CreatedOn = token.CreatedOn, ExpiresOn = token.ExpiresOn };

		}

	}
}
