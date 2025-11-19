using Foodo.API.Models.Request;
using Foodo.Application.Abstraction;
using Foodo.Application.Models.Input;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Foodo.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		private readonly IAuthenticationService _service;

		public AuthenticationController(IAuthenticationService service)
		{
			_service = service;
		}
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromForm] LoginRequest request)
		{
			return Ok();
		}
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
		[HttpPost("change-password")]
		public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordRequest request)
		{
			return Ok();
		}
		[HttpPost("forget-password")]
		public async Task<IActionResult> ForgetPassword([FromForm] ForgetPasswordRequest request)
		{
			return Ok();
		}
	}
}
