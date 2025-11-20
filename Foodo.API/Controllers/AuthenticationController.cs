using Foodo.API.Models.Request;
using Foodo.Application.Abstraction;
using Foodo.Application.Models.Input;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodo.API.Controllers
{

	/// <summary>
	/// Provides authentication and authorization related endpoints such as login,
	/// registration, password management, and token refreshing.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This controller handles all authentication flows used by the application,
	/// including:
	/// </para>
	/// <list type="bullet">
	///     <item>
	///         <description>User login and token generation</description>
	///     </item>
	///     <item>
	///         <description>User registration for both customers and merchants</description>
	///     </item>
	///     <item>
	///         <description>Change password for authenticated users</description>
	///     </item>
	///     <item>
	///         <description>Forget password flow (submit request &amp; reset password)</description>
	///     </item>
	///     <item>
	///         <description>Refresh access tokens using the stored refresh token</description>
	///     </item>
	/// </list>
	///
	/// <para>
	/// All form-related endpoints use <c>[FromForm]</c>, and some endpoints require
	/// authorization using Bearer JWT tokens.
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
		/// Returns 400 Bad Request if the username or password is incorrect.
		/// </returns>
		/// <remarks>
		/// This endpoint uses form-data (FromForm).
		/// </remarks>

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromForm] LoginRequest request)
		{
			var result = await _service.Login(new LoginInput { Email=request.Email, Password=request.Password });
			if (!result.IsSuccess)
			{
				return BadRequest("Incorrect username or password");
			}
			return Ok(result.Data);
		}

		/// <summary>
		/// Registers a new user (Customer or Merchant).
		/// </summary>
		/// <param name="request">Registration request containing user details.</param>
		/// <returns>
		/// Returns 200 OK with a success message if registration is successful.  
		/// Returns 400 Bad Request with an error message if registration fails.
		/// </returns>
		/// <remarks>
		/// This endpoint uses form-data (FromForm) to receive registration data.
		/// </remarks>

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromForm]RegisterRequest request)
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
				UserType = request.UserType,
				StoreName = request.StoreName,
				StoreDescription = request.StoreDescription,
				City = request.City,
				State = request.State,
				StreetAddress = request.StreetAddress,
				PostalCode = request.PostalCode,
				Country = request.Country
			};
			var result = await _service.Register(input);
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}
			return Ok(result.Message);
		}
		/// <summary>
		/// Changes the password for the currently authenticated user.
		/// </summary>
		/// <param name="request">Change password request containing old and new password.</param>
		/// <returns>
		/// Returns 200 OK if the password was changed successfully.  
		/// Returns 400 Bad Request if the process fails.
		/// </returns>
		/// <remarks>
		/// Requires Authorization (Bearer token).
		/// Uses form-data (FromForm).
		/// </remarks>

		[HttpPost("change-password")]
		[Authorize]
		public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordRequest request)
		{
			return Ok();
		}
		/// <summary>
		/// Sends a password reset code to the user's email.
		/// </summary>
		/// <param name="request">Request containing the email address.</param>
		/// <returns>
		/// Returns 200 OK if the reset request was processed successfully.  
		/// Returns 400 Bad Request if the email does not exist or an error occurs.
		/// </returns>
		/// <remarks>
		/// This endpoint initiates the "Forget Password" process.
		/// </remarks>

		[HttpPost("submit-forget-password-request")]
		public async Task<IActionResult> ForgetPasswordRequest([FromForm] ForgetPasswordRequest request)
		{
			var result=await _service.SubmitForgetPasswordRequest(new SubmitForgetPasswordRequestInput { Email = request.Email });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}
			return Ok(result.Message);
		}
		/// <summary>
		/// Resets the user's password using the verification code.
		/// </summary>
		/// <param name="request">Reset password request containing the reset code and new password.</param>
		/// <returns>
		/// Returns 200 OK if the password was reset successfully.  
		/// Returns 400 Bad Request if the code is invalid or the reset fails.
		/// </returns>
		/// <remarks>
		/// This endpoint completes the "Forget Password" flow.
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
		/// </returns>
		/// <remarks>
		/// Requires Authorization (Bearer token).  
		/// Reads the refresh token from browser cookies.
		/// </remarks>

		[HttpPost("refresh-token")]
		[Authorize]
		public async Task<IActionResult> RefreshToken()
		{
			var RefreshToken= Request.Cookies["RefreshToken"];
			var result = await _service.RefreshToken(RefreshToken);
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}

			return Ok(result.Data);
		}
	}
}
