using Foodo.API.Models.Request;
using Foodo.Application.Abstraction;
using Foodo.Application.Models.Input;
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
		/// <summary>
		/// Logs in a user using email and password.
		/// </summary>
		/// <param name="request">Login request containing email and password.</param>
		/// <returns>
		/// Returns 200 OK with token data if login is successful.  
		/// Returns 401 Unauthorized if email or password is incorrect.
		/// </returns>
		/// <remarks>
		/// This endpoint uses form-data (<c>[FromForm]</c>).
		/// </remarks>


		[HttpPost("login")]
		public async Task<IActionResult> Login([FromForm] LoginRequest request)
		{
			var result = await _service.Login(new LoginInput { Email = request.Email, Password = request.Password });
			if (!result.IsSuccess)
			{
				return Unauthorized("Incorrect username or password");
			}
			return Ok(result.Data);
		}

		/// <summary>
		/// Registers a new customer account.
		/// </summary>
		/// <remarks>
		/// This endpoint creates a new customer user with personal information, 
		/// address details, and assigns the "Customer" role.
		/// </remarks>
		/// <param name="request">
		/// Customer registration data, including email, password, name, gender, 
		/// date of birth, phone number, and address details.
		/// </param>
		/// <returns>
		/// Returns 200 OK if registration succeeds, or 409 Conflict if email/username already exists.
		/// </returns>
		/// <response code="200">Customer registered successfully.</response>
		/// <response code="409">Email or username already in use.</response>


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
			{
				return Conflict(result.Message);
			}
			return Ok(result.Message);
		}
		/// <summary>
		/// Registers a new merchant account.
		/// </summary>
		/// <remarks>
		/// This endpoint creates a new merchant user, including store details,
		/// and assigns the "Merchant" role.
		/// </remarks>
		/// <param name="request">
		/// Merchant registration data including email, password, store name 
		/// and store description.
		/// </param>
		/// <returns>
		/// Returns 200 OK if registration succeeds, or 409 Conflict if email already exists.
		/// </returns>
		/// <response code="200">Merchant registered successfully.</response>
		/// <response code="409">Email already in use.</response>

		[HttpPost("register-Merchant")]
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
			{
				return Conflict(result.Message);
			}
			return Ok(result.Message);

		}
		/// <summary>
		/// Changes the password for the currently authenticated user.
		/// </summary>
		/// <param name="request">Model containing old and new password.</param>
		/// <returns>
		/// Returns 204 No Content if password is changed successfully.  
		/// Returns 400 Bad Request if validation fails.
		/// </returns>
		/// <remarks>
		/// Requires Bearer authentication.  
		/// This endpoint uses form-data (<c>[FromForm]</c>).
		/// </remarks>


		[HttpPost("change-password")]
		[Authorize]
		public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordRequest request)
		{
			var UserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			var result = await _service.ChangePassword(new ChangePasswordInput
			{
				CurrentPassword = request.CurrentPassword,
				NewPassword = request.NewPassword,
				UserId = UserId
			});
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}
			return NoContent();
		}
		/// <summary>
		/// Sends a password reset code to the user's email.
		/// </summary>
		/// <param name="request">Model containing the user's email.</param>
		/// <returns>
		/// Returns 200 OK if the request is submitted successfully.  
		/// Returns 400 Bad Request if the email does not exist.
		/// </returns>
		/// <remarks>
		/// This endpoint initiates the "Forget Password" flow.
		/// </remarks>


		[HttpPost("submit-forget-password-request")]
		public async Task<IActionResult> ForgetPasswordRequest([FromForm] ForgetPasswordRequest request)
		{
			var result = await _service.SubmitForgetPasswordRequest(new SubmitForgetPasswordRequestInput { Email = request.Email });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}
			return Ok(result.Message);
		}
		/// <summary>
		/// Resets the user's password using the verification code.
		/// </summary>
		/// <param name="request">Model containing the reset code and new password.</param>
		/// <returns>
		/// Returns 200 OK if the password reset is successful.  
		/// Returns 400 Bad Request if the code is invalid or expired.
		/// </returns>
		/// <remarks>
		/// Completes the "Forget Password" process.
		/// </remarks>


		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordRequest request)
		{
			var result = await _service.ForgetPassword(new ForgetPasswordInput { Code = request.Code, Password = request.NewPassword });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}
			return Ok(result.Message);
		}
		/// <summary>
		/// Refreshes the access token using the refresh token stored in cookies.
		/// </summary>
		/// <returns>
		/// Returns 200 OK with a new access token if the refresh is successful.  
		/// Returns 400 Bad Request if the refresh token is invalid or expired.  
		/// Returns 401 Unauthorized if the refresh token is missing or the user is not authenticated.
		/// </returns>
		/// <remarks>
		/// Requires Authorization (Bearer token).  
		/// Reads the refresh token from browser cookies.
		/// </remarks>
		/// <response code="200">A new access token was generated successfully.</response>
		/// <response code="400">Invalid or expired refresh token.</response>
		/// <response code="401">Refresh token is missing or user is not authenticated.</response>



		[HttpPost("refresh-token")]
		[Authorize]
		public async Task<IActionResult> RefreshToken()
		{
			var RefreshToken = Request.Cookies["RefreshToken"];
			var result = await _service.RefreshToken(RefreshToken);
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}

			return Ok(result.Data);
		}
		/// <summary>
		/// Sends a verification email request to the authenticated user.
		/// </summary>
		/// <remarks>
		/// This endpoint sends a verification code to the user's email.  
		/// The user must be authenticated.
		/// </remarks>
		/// <returns>
		/// Returns 200 OK if the email was sent successfully.  
		/// Returns 400 Bad Request if the process fails.
		/// </returns>
		/// <response code="200">Verification email sent successfully.</response>
		/// <response code="400">Failed to send verification email.</response>
		/// <response code="401">User is not authenticated.</response>

		[Authorize]
		[HttpPost("verify-email-request")]
		public async Task<IActionResult> VerifyEmail()
		{
			var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
			var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
			var result = await _service.VerifyEmailRequest(new VerifyEmailRequestInput { Email = email ,Role=role});
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}
			return Ok(result.Message);

		}
		/// <summary>
		/// Confirms the email verification using the provided verification code.
		/// </summary>
		/// <param name="request">The verification request containing the code.</param>
		/// <remarks>
		/// Validates the verification code previously sent to the user's email.  
		/// Requires authentication.
		/// </remarks>
		/// <returns>
		/// Returns 200 OK if the email is verified successfully.  
		/// Returns 400 Bad Request if the code is invalid or expired.
		/// </returns>
		/// <response code="200">Email verified successfully.</response>
		/// <response code="400">Invalid or expired verification code.</response>
		/// <response code="401">User is not authenticated.</response>

		[Authorize]
		[HttpPost("verify-email")]
		public async Task<IActionResult> VerifyEmail([FromForm] VerifyEmailRequest request)
		{
			var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
			var result = await _service.VerifyEmail(new VerifyEmailInput { Code = request.Code });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}
			return Ok(result.Message);
		}
	}
}
