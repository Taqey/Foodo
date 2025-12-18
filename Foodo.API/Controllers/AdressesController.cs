using Foodo.API.Models.Request.Profile.Customer;
using Foodo.Application.Abstraction.Profile.CustomerProfile;
using Foodo.Application.Abstraction.Profile.MerchantProfile;
using Foodo.Application.Models.Input.Profile.Customer;
using Foodo.Application.Models.Input.Profile.Merchant;
using Foodo.Application.Models.Response;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Security.Claims;

namespace Foodo.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[EnableRateLimiting("FixedWindowPolicy")]

	public class AdressesController : ControllerBase
	{
		private readonly ICustomerAdressService _customerAdressService;
		private readonly IMerchantAdressService _merchantAdressService;

		public AdressesController(ICustomerAdressService customerAdressService, IMerchantAdressService merchantAdressService)
		{
			_customerAdressService = customerAdressService;
			_merchantAdressService = merchantAdressService;
		}
		private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
		private string Role => User.FindFirst(ClaimTypes.Role)?.Value;

		#region Add Address

		/// <summary>
		/// Adds a new address to the User's profile.
		/// </summary>
		/// <param name="request">Address details.</param>
		/// <returns>Status message.</returns>
		/// <response code="200">Address added successfully.</response>
		/// <response code="400">Failed to add address.</response>
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> AddAddress([FromBody] AddAdressRequest request)
		{
			ApiResponse result = null;
			if (Role == nameof(UserType.Customer))
			{

				result = await _customerAdressService.AddAdress(new CustomerAddAdressInput
				{
					CustomerId = UserId,
					Adresses = request.Adresses
				});
			}
			else
			{
				result = await _merchantAdressService.AddAdress(new MerchantAddAdressInput
				{
					MerchantId = UserId,
					Adresses = request.Adresses
				});
			}
			if (!result.IsSuccess)
			{
				Log.Warning("Failed to add address: {Message}", result.Message);
				return BadRequest(result.Message);
			}

			return Ok(result.Message);
		}

		#endregion

		#region Delete Address

		/// <summary>
		/// Deletes an address from the User's profile.
		/// </summary>
		/// <param name="id">ID of the address to delete.</param>
		/// <returns>Status message.</returns>
		/// <response code="200">Address deleted successfully.</response>
		/// <response code="400">Failed to delete address.</response>
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> DeleteAddress(int id)
		{
			ApiResponse result = null;
			if (Role == nameof(UserType.Customer))
			{

				result = await _customerAdressService.RemoveAdress(new CustomerRemoveAdressInput
				{
					CustomerId = UserId,
					adressId = id
				});
			}
			else
			{
				result = await _merchantAdressService.RemoveAdress(new MerchantRemoveAdressInput { AdressId = id, MerchantId = UserId });
			}

			if (!result.IsSuccess)
			{
				Log.Warning("Failed to delete address {AddressId}: {Message}", id, result.Message);
				return BadRequest(result.Message);
			}

			return Ok(result.Message);
		}

		#endregion

		#region Set Default Address

		/// <summary>
		/// Sets a specific address as the default address for the User.
		/// </summary>
		/// <param name="id">Address ID.</param>
		/// <returns>Status message.</returns>
		/// <response code="200">Default address set successfully.</response>
		/// <response code="400">Failed to set default address.</response>
		[HttpPut("{id}/default")]
		[Authorize(Roles = nameof(UserType.Customer))]
		public async Task<IActionResult> SetDefaultAddress(int id)
		{

			var result = await _customerAdressService.MakeAdressDefault(new CustomerMakeAdressDefaultInput
			{
				CustomerId = UserId,
				AdressId = id
			});

			if (!result.IsSuccess)
			{
				Log.Warning("Failed to set default address {AddressId}: {Message}", id, result.Message);
				return BadRequest(result.Message);
			}

			return Ok(result.Message);
		}

		#endregion
	}
}
