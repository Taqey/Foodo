using Foodo.Application.Abstraction.Authentication;
using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Models.Dto.Auth;
using Foodo.Application.Models.Input.Auth;
using Foodo.Application.Models.Input.Merchant;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Enums;
using Foodo.Domain.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Security.Cryptography;

namespace Foodo.Application.Implementation.Authentication
{
	public class AuthenticationService : IAuthenticationService
	{
		private readonly IUserService _userService;
		private readonly ICreateToken _createToken;
		private readonly IHttpContextAccessor _http;
		private readonly IEmailSenderService _senderService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICacheService _cacheService;

		public AuthenticationService(IUserService userService, ICreateToken createToken, IHttpContextAccessor http,IEmailSenderService senderService,IUnitOfWork unitOfWork,ICacheService cacheService)
		{
			_userService = userService;
			_createToken = createToken;
			_http = http;
			_senderService = senderService;
			_unitOfWork = unitOfWork;
			_cacheService = cacheService;
		}

		#region Register
		public async Task<ApiResponse<UserIdDto>> Register(RegisterInput input)
		{
			// Start a transaction
			await using var transaction = await _unitOfWork.BeginTransactionAsync();

			try
			{
				// 1) Check if email exists
				var existingUser = await _userService.GetByEmailAsync(input.Email);
				if (existingUser != null)
					return ApiResponse<UserIdDto>.Failure("Email is already in use.");

				// 2) Check if username exists
				existingUser = await _userService.GetByUsernameAsync(input.UserName);
				if (existingUser != null)
					return ApiResponse<UserIdDto>.Failure("Username is already taken.");

				// 3) Create base identity user
				var user = new ApplicationUser
				{
					Email = input.Email,
					UserName = input.UserName,
					PhoneNumber = input.PhoneNumber
				};

				var result = await _userService.CreateUserAsync(user, input.Password);
				if (!result.Succeeded)
				{
					return ApiResponse<UserIdDto>.Failure("User creation failed: "
						+ string.Join(", ", result.Errors.Select(e => e.Description)));
				}

				string role = "";

				// 4) Add Customer or Merchant
				if (input.UserType == UserType.Customer)
				{
					user.TblCustomer = new TblCustomer
					{
						FirstName = input.FirstName,
						LastName = input.LastName,
						Gender = input.Gender.ToString(),
						BirthDate = (DateOnly)input.DateOfBirth
					};

					user.TblAdresses.Add(new TblAdress
					{
						City = input.City,
						State = input.State,
						PostalCode = input.PostalCode,
						Country = input.Country,
						IsDefault = true,
						StreetAddress = input.StreetAddress,
					});

					role = "Customer";
				}
				else
				{
					user.TblMerchant = new TblMerchant
					{
						StoreName = input.StoreName,
						StoreDescription = input.StoreDescription
					};

					role = "Merchant";
				}

				// 5) Save customer/merchant data
				await _unitOfWork.saveAsync();

				// 6) Add Role
				result = await _userService.AddRolesToUser(user, role);
				if (!result.Succeeded)
				{
					return ApiResponse<UserIdDto>.Failure("Failed to assign role: "
						+ string.Join(", ", result.Errors.Select(e => e.Description)));
				}

				// 7) Commit transaction
				await transaction.CommitAsync();

				// 8) Clear cache if merchant
				if (input.UserType == UserType.Merchant)
				{
					_cacheService.RemoveByPrefix("customer_merchant:list:");
				}

				return ApiResponse<UserIdDto>.Success(new UserIdDto { UserId = user.Id }, "User registered successfully.");
			}
			catch
			{
				// Rollback on error
				await transaction.RollbackAsync();
				return ApiResponse<UserIdDto>.Failure("User registration failed.");
			}
		}
		public async Task<ApiResponse> AddCategory(CategoryInput input)
		{
			var user = _unitOfWork.UserCustomRepository.ReadMerchants().Where(e => e.Id == input.UserId).FirstOrDefault();
			//var user=await _userService.GetByIdAsync(input.UserId);
			foreach (var category in input.restaurantCategories)
			{
				user.TblMerchant.TblRestaurantCategories.Add(new TblRestaurantCategory
				{
					categoryid = (int)category
				});

			}
			await _userService.UpdateAsync(user);
			return ApiResponse.Success("Categories added successfully.");
		}

		#endregion
		
		#region Login

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
		public async Task<ApiResponse<JwtDto>> RefreshToken(string Token)
		{
			var user = await _userService.GetUserByRefreshToken(Token);
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
				Secure = true,
				Path = "/"
			};

			_http.HttpContext.Response.Cookies.Append("RefreshToken", RefreshToken.Token, cookieOptions);
			user.lkpRefreshTokens.Add(new lkpRefreshToken { Token = RefreshToken.Token, CreatedAt = RefreshToken.CreatedOn, ExpiresAt = RefreshToken.ExpiresOn });
			await _userService.UpdateAsync(user);
			return new JwtDto { Token = token.Token, CreatedOn = token.CreatedOn, ExpiresOn = token.ExpiresOn };

		}
		#endregion

		#region PasswordManagment

		public async Task<ApiResponse> ChangePassword(ChangePasswordInput input)
		{
			var user= await _userService.GetByIdAsync(input.UserId);
			var result= await _userService.ChangePasswordAsync(user,input.CurrentPassword, input.NewPassword);
			if(!result.Succeeded)
			{
				return ApiResponse.Failure("Password change failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
			}
			return ApiResponse.Success("Password changed successfully.");
		}

		public async Task<ApiResponse> ForgetPassword(ForgetPasswordInput input)
		{

			var user=await _userService.GetUserByResetCode(input.Code);
			if(user==null)
			{
				return  new ApiResponse {  Message = "Invalid or expired reset code." };
			}
			var code= user.LkpCodes.FirstOrDefault(e=>e.Key==input.Code);
			if (code.ExpiresAt < DateTime.UtcNow)
			{
				return new ApiResponse { Message = "Invalid or expired reset code." };
			}
			if ((bool)code.IsUsed)
			{
				return ApiResponse.Failure("Reset Code Already used");
			}
			var result=	await _userService.ForgetPasswordAsync(user, input.Password);
			if(result.Succeeded)
			{
				code.IsUsed= true;
				return ApiResponse.Success("Password has been reset successfully.");
			} 
				return ApiResponse.Failure("Password reset failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
			
		}

		public async Task<ApiResponse> SubmitForgetPasswordRequest(SubmitForgetPasswordRequestInput input)
		{
			var user=await _userService.GetInclude(input.Email,e=>e.TblCustomer,e=>e.TblMerchant);
			if(user==null)
			{
				return ApiResponse.Failure("Email not found");
			}
			var role=(await _userService.GetRolesForUser(user)).FirstOrDefault();
			string Name;
			if (role == (UserType.Merchant).ToString())
			{
				Name = user.TblMerchant.StoreName;
			}
			else
			{
				Name = user.TblCustomer.FirstName;
			}
			var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
			user.LkpCodes.Add(new LkpCode { Key = code, CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddMinutes(10), CodeType = CodeType.PasswordReset });
			await _userService.UpdateAsync(user);
			var result=await _senderService.SendEmailAsync(input.Email, Name, "Password Reset", $"Your Reset Password code is {code}");
			return result;
		}
		#endregion

		#region EmailVerification

		public async Task<ApiResponse> VerifyEmailRequest(VerifyEmailRequestInput input)
		{
			var user = await _userService.GetInclude(input.Email, e => e.TblCustomer, e => e.TblMerchant);
			if (user == null)
			{
				return ApiResponse.Failure("Email not found");
			}
			var role = input.Role;
			string Name;
			if (role == (UserType.Merchant).ToString())
			{
				Name = user.TblMerchant.StoreName;
			}
			else
			{
				Name = user.TblCustomer.FirstName;
			}
			var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
			user.LkpCodes.Add(new LkpCode { Key = code, CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddMinutes(10), CodeType = CodeType.EmailVerification });
			await _userService.UpdateAsync(user);

			var result =_senderService.SendEmailAsync(input.Email, "User", "Verify your email", $"Your Email Verification code is {code}");
			return ApiResponse.Success("Verification email sent.");
		}

		public async Task<ApiResponse> VerifyEmail(VerifyEmailInput input)
		{
			var user = await _userService.GetUserByVerificationToken(input.Code);
			if (user == null)
			{
				return new ApiResponse { Message = "Invalid or expired verification token." };
			}
			var code = user.LkpCodes.FirstOrDefault(e => e.Key == input.Code);
			if (code.ExpiresAt < DateTime.UtcNow)
			{
				return new ApiResponse { Message = "Invalid or expired verification token." };
			}
			if ((bool)code.IsUsed)
			{
				return ApiResponse.Failure("Verification token already used.");
			}
			code.IsUsed = true;
			user.EmailConfirmed = true;
			await _userService.UpdateAsync(user);

				return ApiResponse.Success("Email has been verified successfully.");
			
		}
		#endregion

	}
}
