using Foodo.API.Models.Request.Profile.Merchant;
using Foodo.Application.Abstraction.Profile.MerchantProfile;
using Foodo.Application.Models.Input.Profile.Merchant;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Foodo.API.Controllers
{
	/// <summary>
	/// Provides profile management operations for merchants, including 
	/// retrieving profile information and managing merchant addresses.
	/// </summary>
	/// <remarks>
	/// All endpoints require Bearer authentication with the <c>Merchant</c> role.
	/// </remarks>
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = nameof(UserType.Merchant))]
	[EnableRateLimiting("FixedWindowPolicy")]

	public class MerchantProfileController : ControllerBase
	{
		private readonly IMerchantProfileService _service;
		private readonly ILogger<MerchantProfileController> _logger;

		public MerchantProfileController(IMerchantProfileService service, ILogger<MerchantProfileController> logger)
		{
			_service = service;
			_logger = logger;
		}

		#region Get Profile
		[HttpGet("get-merchant-profile")]
		public async Task<IActionResult> GetProfile()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			_logger.LogInformation("Get merchant profile attempt started | MerchantId={MerchantId} | TraceId={TraceId}", userId, HttpContext.TraceIdentifier);

			var result = await _service.GetMerchantProfile(new MerchantProfileInput { MerchantId = userId });

			if (!result.IsSuccess)
			{
				_logger.LogWarning("Get merchant profile failed | Reason={Reason} | TraceId={TraceId}", result.Message, HttpContext.TraceIdentifier);
				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}

			_logger.LogInformation("Get merchant profile succeeded | MerchantId={MerchantId} | TraceId={TraceId}", userId, HttpContext.TraceIdentifier);
			return Ok(new
			{
				message = "Profile retrieved successfully",
				traceId = HttpContext.TraceIdentifier,
				data = result.Data
			});
		}
		#endregion

		#region Add Address
		[HttpPost("add-adress")]
		public async Task<IActionResult> AddAddress([FromBody] MerchantAddAdressRequest request)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			_logger.LogInformation("Add merchant address attempt started | MerchantId={MerchantId} | TraceId={TraceId}", userId, HttpContext.TraceIdentifier);

			if (!ModelState.IsValid)
			{
				var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
				_logger.LogWarning("Add merchant address failed: Invalid model | Errors={Errors} | TraceId={TraceId}", errors, HttpContext.TraceIdentifier);
				return BadRequest(new { message = "Invalid request data", errors, traceId = HttpContext.TraceIdentifier });
			}

			var result = await _service.AddAdress(new MerchantAddAdressInput { MerchantId = userId, Adresses = request.Adresses });

			if (!result.IsSuccess)
			{
				_logger.LogWarning("Add merchant address failed | Reason={Reason} | TraceId={TraceId}", result.Message, HttpContext.TraceIdentifier);
				return BadRequest(new { message = result.Message, traceId = HttpContext.TraceIdentifier });
			}

			_logger.LogInformation("Add merchant address succeeded | MerchantId={MerchantId} | TraceId={TraceId}", userId, HttpContext.TraceIdentifier);
			return Ok(new { message = "Address added successfully", traceId = HttpContext.TraceIdentifier });
		}
		#endregion

		#region Delete Address
		[HttpDelete("delete-adress/{id}")]
		public async Task<IActionResult> DeleteAddress(int id)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			_logger.LogInformation("Delete merchant address attempt started | MerchantId={MerchantId} | AddressId={AddressId} | TraceId={TraceId}", userId, id, HttpContext.TraceIdentifier);

			var result = await _service.RemoveAdress(new MerchantRemoveAdressInput { MerchantId = userId, AdressId = id });

			if (!result.IsSuccess)
			{
				_logger.LogWarning("Delete merchant address failed | Reason={Reason} | MerchantId={MerchantId} | AddressId={AddressId} | TraceId={TraceId}", result.Message, userId, id, HttpContext.TraceIdentifier);
				return BadRequest(new { message = result.Message, traceId = HttpContext.TraceIdentifier });
			}

			_logger.LogInformation("Delete merchant address succeeded | MerchantId={MerchantId} | AddressId={AddressId} | TraceId={TraceId}", userId, id, HttpContext.TraceIdentifier);
			return Ok(new { message = "Address deleted successfully", traceId = HttpContext.TraceIdentifier });
		}
		#endregion
	}
}
