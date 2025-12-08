using Foodo.API.Models.Request.Profile.Customer;
using Foodo.Application.Abstraction.Profile.CustomerProfile;
using Foodo.Application.Models.Input.Profile.Customer;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Foodo.API.Controllers
{
	/// <summary>
	/// Provides profile management operations for customers such as 
	/// retrieving profile information, managing addresses, and setting default addresses.
	/// </summary>
	/// <remarks>
	/// All endpoints require authenticated users with the <c>Customer</c> role.
	/// </remarks>
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(Roles = nameof(UserType.Customer))]
	[EnableRateLimiting("FixedWindowPolicy")]

	public class CustomerProfileController : ControllerBase
	{
		private readonly ICustomerProfileService _service;
		private readonly ILogger<CustomerProfileController> _logger;

		public CustomerProfileController(ICustomerProfileService service, ILogger<CustomerProfileController> logger)
		{
			_service = service;
			_logger = logger;
		}

		// Helper to get UserId
		private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

		#region Get Profile

		/// <summary>
		/// Retrieves the profile details of the authenticated customer.
		/// </summary>
		/// <returns>Customer profile data.</returns>
		/// <response code="200">Profile retrieved successfully.</response>
		/// <response code="400">Failed to retrieve profile.</response>
		[HttpGet("profile")]
		public async Task<IActionResult> GetProfile()
		{
			_logger.LogInformation("Fetching profile for CustomerId: {CustomerId}", UserId);

			var result = await _service.GetCustomerProfile(new CustomerGetCustomerProfileInput
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

		#region Add Address

		/// <summary>
		/// Adds a new address to the customer's profile.
		/// </summary>
		/// <param name="request">Address details.</param>
		/// <returns>Status message.</returns>
		/// <response code="200">Address added successfully.</response>
		/// <response code="400">Failed to add address.</response>
		[HttpPost("address")]
		public async Task<IActionResult> AddAddress([FromBody] CustomerAddAdressRequest request)
		{
			_logger.LogInformation("Adding address for CustomerId: {CustomerId}", UserId);

			var result = await _service.AddAdress(new CustomerAddAdressInput
			{
				CustomerId = UserId,
				Adresses = request.Adresses
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning("Failed to add address: {Message}", result.Message);
				return BadRequest(result.Message);
			}

			return Ok(result.Message);
		}

		#endregion

		#region Delete Address

		/// <summary>
		/// Deletes an address from the customer's profile.
		/// </summary>
		/// <param name="id">ID of the address to delete.</param>
		/// <returns>Status message.</returns>
		/// <response code="200">Address deleted successfully.</response>
		/// <response code="400">Failed to delete address.</response>
		[HttpDelete("address/{id}")]
		public async Task<IActionResult> DeleteAddress(int id)
		{
			_logger.LogInformation("Deleting address {AddressId} for CustomerId: {CustomerId}", id, UserId);

			var result = await _service.RemoveAdress(new CustomerRemoveAdressInput
			{
				CustomerId = UserId,
				adressId = id
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning("Failed to delete address {AddressId}: {Message}", id, result.Message);
				return BadRequest(result.Message);
			}

			return Ok(result.Message);
		}

		#endregion

		#region Set Default Address

		/// <summary>
		/// Sets a specific address as the default address for the customer.
		/// </summary>
		/// <param name="id">Address ID.</param>
		/// <returns>Status message.</returns>
		/// <response code="200">Default address set successfully.</response>
		/// <response code="400">Failed to set default address.</response>
		[HttpPut("address/default/{id}")]
		public async Task<IActionResult> SetDefaultAddress(int id)
		{
			_logger.LogInformation("Setting default address {AddressId} for CustomerId: {CustomerId}", id, UserId);

			var result = await _service.MakeAdressDefault(new CustomerMakeAdressDefaultInput
			{
				CustomerId = UserId,
				AdressId = id
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning("Failed to set default address {AddressId}: {Message}", id, result.Message);
				return BadRequest(result.Message);
			}

			return Ok(result.Message);
		}

		#endregion
	}
}
