using Foodo.API.Models.Request.Authentication;
using Foodo.Application.Commands.Authentication.AddCategory;
using Foodo.Application.Commands.Authentication.ChangePassword;
using Foodo.Application.Commands.Authentication.ForgetPassword;
using Foodo.Application.Commands.Authentication.Login;
using Foodo.Application.Commands.Authentication.RefreshToken;
using Foodo.Application.Commands.Authentication.Register;
using Foodo.Application.Commands.Authentication.SubmitForgetPasswordRequest;
using Foodo.Application.Commands.Authentication.VerifyEmail;
using Foodo.Application.Commands.Authentication.VerifyEmailRequest;
using Foodo.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
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
	[EnableRateLimiting("SlidingWindowPolicy")]

	public class AuthenticationController : ControllerBase
	{
		private readonly IMediator _mediator;

		public AuthenticationController(IMediator mediator)
		{
			_mediator = mediator;
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
			var result = await _mediator.Send(new LoginCommand
			{
				Email = request.Email,
				Password = request.Password
			});
			if (!result.IsSuccess)
			{
				Log.Warning("Login failed | Email={Email} | Reason={Reason} | TraceId={TraceId}", request.Email, result.Message, HttpContext.TraceIdentifier);

				return Unauthorized(new
				{
					message = "Incorrect username or password",
					traceId = HttpContext.TraceIdentifier
				});
			}
			return Ok(new
			{
				message = "Login successful",
				traceId = HttpContext.TraceIdentifier,
				data = result.Data
			});
		}
		#endregion

		#region REGISTRATION

		/// <summary>
		/// Registers a new customer user.
		/// </summary>
		/// <returns>A success message if registration is completed.</returns>
		/// <response code="201">Customer registered successfully.</response>
		/// <response code="400">Invalid registration data.</response>
		/// <response code="409">Email or username already exists.</response>
		[HttpPost("register-customer")]
		public async Task<IActionResult> RegisterCustomer([FromForm] CustomerRegisterRequest request)
		{
			var input = new RegisterCommand
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
			var result = await _mediator.Send(input);
			if (!result.IsSuccess)
			{
				Log.Warning("Customer registration failed | Email={Email} | Reason={Reason} | TraceId={TraceId}", request.Email, result.Message, HttpContext.TraceIdentifier);
				return Conflict(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
			return StatusCode(StatusCodes.Status201Created, new
			{
				message = "Customer registered successfully",
				traceId = HttpContext.TraceIdentifier,
			});
		}


		/// <summary>
		/// Registers a new merchant user.
		/// </summary>
		/// <returns>Merchant info if registration is successful.</returns>
		/// <response code="201">Merchant registered successfully.</response>
		/// <response code="400">Invalid registration data.</response>
		/// <response code="409">Email or username already exists.</response>
		[HttpPost("register-merchant")]
		public async Task<IActionResult> RegisterMerchant([FromForm] MerchantRegisterRequest request)
		{
			var input = new RegisterCommand
			{
				Email = request.Email,
				Password = request.Password,
				StoreName = request.StoreName,
				StoreDescription = request.StoreDescription,
				UserType = UserType.Merchant,
				UserName = request.UserName
			};
			var result = await _mediator.Send(input);
			if (!result.IsSuccess)
			{
				Log.Warning("Merchant registration failed | Email={Email} | Reason={Reason} | TraceId={TraceId}", request.Email, result.Message, HttpContext.TraceIdentifier);
				return Conflict(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
			return StatusCode(StatusCodes.Status201Created, new
			{
				message = "Merchant registered successfully",
				traceId = HttpContext.TraceIdentifier,
				data = result.Data
			});
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
			var result = await _mediator.Send(new AddCategoryCommand
			{
				UserId = request.UserId,
				restaurantCategories = request.RestaurantCategories
			});
			if (!result.IsSuccess)
			{
				Log.Warning("Add category failed | UserId={UserId} | Reason={Reason} | TraceId={TraceId}", request.UserId, result.Message, HttpContext.TraceIdentifier);
				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
			return Ok(new
			{
				message = "Categories added successfully",
				traceId = HttpContext.TraceIdentifier
			});
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
			var result = await _mediator.Send(new ChangePasswordCommand
			{
				CurrentPassword = request.CurrentPassword,
				NewPassword = request.NewPassword,
				UserId = userId
			});

			if (!result.IsSuccess)
			{
				Log.Warning("Change password failed | UserId={UserId} | Reason={Reason} | TraceId={TraceId}", userId, result.Message, HttpContext.TraceIdentifier);
				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
			return NoContent();
		}

		/// <summary>
		/// Sends a password reset code to the user's email.
		/// </summary>
		/// <returns>A success message indicating the email was sent.</returns>
		/// <response code="200">Password reset code sent successfully.</response>
		/// <response code="400">Failed to send password reset code.</response>
		/// <response code="404">Email not found.</response>
		[HttpPost("submit-forget-password-request")]
		public async Task<IActionResult> SubmitForgetPasswordRequest([FromForm] ForgetPasswordRequest request)
		{
			var result = await _mediator.Send(new SubmitForgetPasswordRequestCommand { Email = request.Email });

			if (!result.IsSuccess)
			{
				Log.Warning("Forget password request failed | Email={Email} | Reason={Reason} | TraceId={TraceId}", request.Email, result.Message, HttpContext.TraceIdentifier);
				if (result.Message.Contains("found"))
				{
					return NotFound(new
					{
						message = result.Message,
						traceId = HttpContext.TraceIdentifier
					});
				}
				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
			return Ok(new
			{
				message = "Forget password request submitted successfully",
				traceId = HttpContext.TraceIdentifier
			});
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
			var result = await _mediator.Send(new ForgetPasswordCommand
			{
				Code = request.Code,
				Password = request.NewPassword
			});

			if (!result.IsSuccess)
			{
				Log.Warning("Reset password failed | Code={Code} | Reason={Reason} | TraceId={TraceId}", request.Code, result.Message, HttpContext.TraceIdentifier);
				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
			return Ok(new
			{
				message = "Password reset successfully",
				traceId = HttpContext.TraceIdentifier
			});
		}

		#endregion

		#region TOKEN MANAGEMENT

		/// <summary> Refreshes the access token using the stored refresh token.</summary>
		/// <returns>The new access token if refresh is successful.</returns>
		/// <response code="200">Token refreshed successfully.</response>
		/// <response code="400">Failed to refresh token.</response>
		/// <response code="401">User not authenticated.</response>
		[HttpPost("refresh-token")]
		public async Task<IActionResult> RefreshToken()
		{
			var refreshToken = Request.Cookies["RefreshToken"];
			if (string.IsNullOrEmpty(refreshToken))
			{
				Log.Warning("Refresh token not found. Please login again.", HttpContext.TraceIdentifier);
				return BadRequest(new
				{
					message = "Refresh token not found. Please login again.",
					traceId = HttpContext.TraceIdentifier
				});
			}
			var result = await _mediator.Send(new RefreshTokenCommand { Token = refreshToken });

			if (!result.IsSuccess)
			{
				Log.Warning("Refresh token failed | Reason={Reason} | TraceId={TraceId}", result.Message, HttpContext.TraceIdentifier);
				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
			return Ok(new
			{
				message = "Token refreshed successfully",
				traceId = HttpContext.TraceIdentifier,
				data = result.Data
			});
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
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var email = User.FindFirst(ClaimTypes.Email)?.Value;
			var role = User.FindFirst(ClaimTypes.Role)?.Value;
			var result = await _mediator.Send(new VerifyEmailRequestCommand
			{
				Email = email,
				Role = role,
			});

			if (!result.IsSuccess)
			{
				Log.Warning("Verify email request failed | UserId={UserId} | Email={Email} | Role={Role} | Reason={Reason} | TraceId={TraceId}", userId, email, role, result.Message, HttpContext.TraceIdentifier);
				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
			return Ok(new
			{
				message = "Email verification request submitted successfully",
				traceId = HttpContext.TraceIdentifier
			});
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
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var result = await _mediator.Send(new VerifyEmailCommand
			{
				Code = request.Code
			});

			if (!result.IsSuccess)
			{
				Log.Warning("Email verification failed | UserId={UserId} | Code={Code} | Reason={Reason} | TraceId={TraceId}", userId, request.Code, result.Message, HttpContext.TraceIdentifier);

				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}

			return Ok(new
			{
				message = "Email verified successfully",
				traceId = HttpContext.TraceIdentifier
			});
		}


		#endregion
	}
}
