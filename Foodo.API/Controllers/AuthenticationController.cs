using Foodo.API.Models.Request.Authentication;
using Foodo.Application.Abstraction.Authentication;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Auth;
using Foodo.Application.Models.Input.Merchant;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Foodo.API.Controllers
{
	/// <summary>
	/// Provides authentication and authorization related endpoints such as login,
	/// registration, password management, email verification, and token refreshing.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This controller handles all authentication flows used by the application,
	/// including:
	/// </para>
	/// <list type="bullet">
	///     <item>
	///         <description>User login and JWT token generation</description>
	///     </item>
	///     <item>
	///         <description>Customer and Merchant registration</description>
	///     </item>
	///     <item>
	///         <description>Password change for authenticated users</description>
	///     </item>
	///     <item>
	///         <description>Forget password flow (request + reset)</description>
	///     </item>
	///     <item>
	///         <description>Email verification request and confirmation</description>
	///     </item>
	///     <item>
	///         <description>Refreshing expired access tokens</description>
	///     </item>
	/// </list>
	/// <para>
	/// All input models are received using <c>[FromForm]</c> when needed, and protected
	/// endpoints require Bearer authentication.
	/// </para>
	/// </remarks>

	[Route("api/[controller]")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		private readonly IAuthenticationService _service;

		public AuthenticationController(IAuthenticationService service)
		{
			_service = service;
		}

		#region LOGIN

		/// <summary>
		/// Logs in a user using email and password.
		/// </summary>
		/// <returns>Authenticated user data if login succeeds.</returns>
		/// <response code="200">Login successful.</response>
		/// <response code="401">Incorrect username or password.</response>
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromForm] LoginRequest request)
		{
			var result = await _service.Login(new LoginInput
			{
				Email = request.Email,
				Password = request.Password
			});

			if (!result.IsSuccess)
				return Unauthorized("Incorrect username or password");

			return Ok(result.Data);
		}

		#endregion

		#region REGISTRATION

		/// <summary>
		/// Registers a new customer user.
		/// </summary>
		/// <returns>A success message if registration is completed.</returns>
		/// <response code="200">Customer registered successfully.</response>
		/// <response code="409">Email or username already exists.</response>
		/// <response code="400">Invalid registration data.</response>
		[HttpPost("register-customer")]
		public async Task<IActionResult> RegisterCustomer([FromForm] CustomerRegisterRequest request)
		{
			var input = new RegisterInput
			{
				Email = request.Email,
				Password = request.Password,
				FirstName = request.FirstName,
				LastName = request.LastName,
				PhoneNumber = request.PhoneNumber,
				Gender = request.Gender,
				DateOfBirth = request.DateOfBirth,
				UserName = request.UserName,
				UserType = UserType.Customer,
				City = request.City,
				State = request.State,
				StreetAddress = request.StreetAddress,
				PostalCode = request.PostalCode,
				Country = request.Country
			};

			var result = await _service.Register(input);

			if (!result.IsSuccess)
				return Conflict(result.Message);

			return Ok(result.Message);
		}

		/// <summary>
		/// Registers a new merchant user.
		/// </summary>
		/// <returns>Merchant info if registration is successful.</returns>
		/// <response code="200">Merchant registered successfully.</response>
		/// <response code="409">Email or username already exists.</response>
		/// <response code="400">Invalid registration data.</response>
		[HttpPost("register-merchant")]
		public async Task<IActionResult> RegisterMerchant([FromForm] MerchantRegisterRequest request)
		{
			var input = new RegisterInput
			{
				Email = request.Email,
				Password = request.Password,
				StoreName = request.StoreName,
				StoreDescription = request.StoreDescription,
				UserType = UserType.Merchant,
				UserName = request.UserName
			};

			var result = await _service.Register(input);

			if (!result.IsSuccess)
				return Conflict(result.Message);

			return Ok(result.Data);
		}

		/// <summary>
		/// Adds restaurant categories to a merchant account.
		/// </summary>
		/// <returns>A success message if categories were added.</returns>
		/// <response code="200">Categories added successfully.</response>
		/// <response code="400">Failed to add categories.</response>
		[HttpPost("add-category")]
		public async Task<IActionResult> AddCategory([FromBody] AddCategoryRequest request)
		{
			var categories = request.restaurantCategories.Split(',')
				.Select(c => Enum.Parse<RestaurantCategory>(c.Trim()))
				.ToList();

			var result = await _service.AddCategory(new CategoryInput
			{
				UserId = request.UserId,
				restaurantCategories = categories
			});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}

		#endregion

		#region PASSWORD MANAGEMENT

		/// <summary>
		/// Changes the password for the authenticated user.
		/// </summary>
		/// <returns>No content if password is changed successfully.</returns>
		/// <response code="204">Password changed successfully.</response>
		/// <response code="400">Failed to change password.</response>
		/// <response code="401">User not authenticated.</response>
		[Authorize]
		[HttpPost("change-password")]
		public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordRequest request)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var result = await _service.ChangePassword(new ChangePasswordInput
			{
				CurrentPassword = request.CurrentPassword,
				NewPassword = request.NewPassword,
				UserId = userId
			});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return NoContent();
		}

		/// <summary>
		/// Sends a password reset code to the user's email.
		/// </summary>
		/// <returns>A success message indicating the email was sent.</returns>
		/// <response code="200">Password reset code sent successfully.</response>
		/// <response code="400">Failed to send password reset code.</response>
		[HttpPost("submit-forget-password-request")]
		public async Task<IActionResult> ForgetPasswordRequest([FromForm] ForgetPasswordRequest request)
		{
			var result = await _service.SubmitForgetPasswordRequest(
				new SubmitForgetPasswordRequestInput { Email = request.Email });

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}

		/// <summary>
		/// Resets the user's password using a verification code.
		/// </summary>
		/// <returns>A success message if password is reset.</returns>
		/// <response code="200">Password reset successfully.</response>
		/// <response code="400">Invalid or expired reset code.</response>
		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordRequest request)
		{
			var result = await _service.ForgetPassword(new ForgetPasswordInput
			{
				Code = request.Code,
				Password = request.NewPassword
			});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}

		#endregion

		#region TOKEN MANAGEMENT

		/// <summary>
		/// Refreshes the access token using the stored refresh token.
		/// </summary>
		/// <returns>The new access token if refresh is successful.</returns>
		/// <response code="200">Token refreshed successfully.</response>
		/// <response code="400">Failed to refresh token.</response>
		/// <response code="401">User not authenticated.</response>
		[HttpPost("refresh-token")]
		public async Task<IActionResult> RefreshToken()
		{
			var refreshToken = Request.Cookies["RefreshToken"];
			var result = await _service.RefreshToken(refreshToken);

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Data);
		}

		#endregion

		#region EMAIL VERIFICATION

		/// <summary>
		/// Sends a verification email to the authenticated user.
		/// </summary>
		/// <returns>A success message when the email is sent.</returns>
		/// <response code="200">Verification email sent successfully.</response>
		/// <response code="400">Failed to send verification email.</response>
		/// <response code="401">User not authenticated.</response>
		[Authorize]
		[HttpPost("verify-email-request")]
		public async Task<IActionResult> VerifyEmail()
		{
			var email = User.FindFirst(ClaimTypes.Email)?.Value;
			var role = User.FindFirst(ClaimTypes.Role)?.Value;

			var result = await _service.VerifyEmailRequest(new VerifyEmailRequestInput
			{
				Email = email,
				Role = role
			});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}

		/// <summary>
		/// Verifies the user's email using a verification code.
		/// </summary>
		/// <returns>A success message if email is verified.</returns>
		/// <response code="200">Email verified successfully.</response>
		/// <response code="400">Invalid or expired verification code.</response>
		/// <response code="401">User not authenticated.</response>
		[Authorize]
		[HttpPost("verify-email")]
		public async Task<IActionResult> VerifyEmail([FromForm] VerifyEmailRequest request)
		{
			var result = await _service.VerifyEmail(new VerifyEmailInput
			{
				Code = request.Code
			});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}

		#endregion
	}
}
