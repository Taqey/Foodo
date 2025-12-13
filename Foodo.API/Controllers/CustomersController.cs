using Foodo.API.Models.Request;
using Foodo.API.Models.Request.Customer;
using Foodo.Application.Abstraction.Customer;
using Foodo.Application.Abstraction.Profile.CustomerProfile;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Input.Profile.Customer;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Foodo.API.Controllers
{
	/// <summary>
	/// Provides endpoints for customers to interact with products, shops, and orders.
	/// </summary>
	/// <remarks>
	/// This controller handles all customer-related operations including:
	/// <list type="bullet">
	///     <item>
	///         <description>Reading products (all, by ID, by category, or by restaurant)</description>
	///     </item>
	///     <item>
	///         <description>Reading shops (all, by ID, or by category)</description>
	///     </item>
	///     <item>
	///         <description>Placing, editing, cancelling, and retrieving orders</description>
	///     </item>
	/// </list>
	/// Protected endpoints require Bearer authentication and the "Customer" role where applicable.
	/// </remarks>
	[Route("api/[controller]")]
	[ApiController]
	public class CustomersController : ControllerBase
	{
		private readonly ICustomerService _service;
		private readonly ICustomerAdressService _profileService;
		private readonly ILogger<CustomersController> _logger;

		public CustomersController(ICustomerService service,ICustomerAdressService profileService, ILogger<CustomersController> logger)
		{
			_service = service;
			_profileService = profileService;
			_logger = logger;
		}

		#region Get Profile

		/// <summary>
		/// Retrieves the profile details of the authenticated customer.
		/// </summary>
		/// <returns>Customer profile data.</returns>
		/// <response code="200">Profile retrieved successfully.</response>
		/// <response code="400">Failed to retrieve profile.</response>
		[HttpGet("profile")]
		[Authorize(Roles =nameof(UserType.Customer))]
		[EnableRateLimiting("FixedWindowPolicy")]
		public async Task<IActionResult> GetProfile()
		{
			var UserId= User.FindFirstValue(ClaimTypes.NameIdentifier);
			_logger.LogInformation("Fetching profile for CustomerId: {CustomerId}", UserId);

			var result = await _profileService.GetCustomerProfile(new CustomerGetCustomerProfileInput
			{
				UserId = UserId
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning("Failed to retrieve customer profile: {Message}", result.Message);
				return BadRequest(result.Message);
			}

			return Ok(result.Data);
		}

		#endregion


	}
}
