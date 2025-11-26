using Foodo.API.Models.Request.Profile.Customer;
using Foodo.API.Models.Request.Profile.Merchant;
using Foodo.Application.Abstraction.Profile.CustomerProfile;
using Foodo.Application.Models.Input.Profile.Customer;
using Foodo.Application.Models.Input.Profile.Merchant;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Foodo.API.Controllers
{
	/// <summary>
	/// Provides profile management operations for customers such as 
	/// retrieving profile information, managing addresses, and setting default addresses.
	/// </summary>
	/// <remarks>
	/// All endpoints require Bearer authentication with the <c>Customer</c> role.
	/// </remarks>
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = nameof(UserType.Customer))]
	public class CustomerProfileController : ControllerBase
	{
		private readonly ICustomerProfileService _service;

		public CustomerProfileController(ICustomerProfileService service)
		{
			_service = service;
		}

		#region Get Profile

		/// <summary>
		/// Retrieves the profile details of the authenticated customer.
		/// </summary>
		/// <returns>Customer profile data.</returns>
		/// <response code="200">Profile retrieved successfully.</response>
		/// <response code="400">Failed to retrieve profile.</response>
		[HttpGet("get-customer-profile")]
		public async Task<IActionResult> Get()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var result = await _service.GetCustomerProfile(new CustomerGetCustomerProfileInput
			{
				UserId = userId
			});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Data);
		}

		#endregion

		#region Add Address

		/// <summary>
		/// Adds a new address to the customer's profile.
		/// </summary>
		/// <param name="request">Address details to add.</param>
		/// <returns>Status message.</returns>
		/// <response code="200">Address added successfully.</response>
		/// <response code="400">Failed to add address.</response>
		[HttpPost("add-adress")]
		public async Task<IActionResult> Post([FromBody] CustomerAddAdressRequest request)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var result = await _service.AddAdress(new CustomerAddAdressInput
			{
				CustomerId = userId,
				Adresses = request.Adresses
			});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

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
		[HttpDelete("delete-adress/{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var result = await _service.RemoveAdress(new CustomerRemoveAdressInput
			{
				CustomerId = userId,
				adressId = id
			});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}

		#endregion

		#region Set Default Address

		/// <summary>
		/// Sets an address as the default address for the customer.
		/// </summary>
		/// <param name="id">ID of the address to set as default.</param>
		/// <returns>Status message.</returns>
		/// <response code="200">Default address set successfully.</response>
		/// <response code="400">Failed to set default address.</response>
		[HttpPut("set-adress-default/{id}")]
		public async Task<IActionResult> Put(int id)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var result = await _service.MakeAdressDefault(new CustomerMakeAdressDefaultInput
			{
				CustomerId = userId,
				AdressId = id
			});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}

		#endregion
	}
}
