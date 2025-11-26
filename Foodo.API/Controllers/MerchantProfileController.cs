using Foodo.API.Models.Request.Profile.Merchant;
using Foodo.Application.Abstraction.Profile.MerchantProfile;
using Foodo.Application.Models.Input.Profile.Merchant;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

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
	public class MerchantProfileController : ControllerBase
	{
		private readonly IMerchantProfileService _service;

		public MerchantProfileController(IMerchantProfileService service)
		{
			_service = service;
		}

		#region Get Profile

		/// <summary>
		/// Retrieves the profile information of the authenticated merchant.
		/// </summary>
		/// <returns>Merchant profile data.</returns>
		/// <response code="200">Profile retrieved successfully.</response>
		/// <response code="400">Failed to retrieve profile.</response>
		[HttpGet("get-merchant-profile")]
		public async Task<IActionResult> GetProfile()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var result = await _service.GetMerchantProfile(new MerchantProfileInput
			{
				MerchantId = userId
			});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Data);
		}

		#endregion

		#region Add Address

		/// <summary>
		/// Adds a new address to the merchant's profile.
		/// </summary>
		/// <param name="request">Address details to add.</param>
		/// <returns>Status message.</returns>
		/// <response code="200">Address added successfully.</response>
		/// <response code="400">Failed to add address.</response>
		[HttpPost("add-adress")]
		public async Task<IActionResult> Post([FromBody] MerchantAddAdressRequest request)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var result = await _service.AddAdress(new MerchantAddAdressInput
			{
				MerchantId = userId,
				Adresses = request.Adresses
			});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}

		#endregion

		#region Delete Address

		/// <summary>
		/// Deletes a specific address from the merchant's profile.
		/// </summary>
		/// <param name="id">ID of the address to delete.</param>
		/// <returns>Status message.</returns>
		/// <response code="200">Address deleted successfully.</response>
		/// <response code="400">Failed to delete address.</response>
		[HttpDelete("delete-adress/{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var result = await _service.RemoveAdress(new MerchantRemoveAdressInput
			{
				MerchantId = userId,
				AdressId = id
			});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}

		#endregion
	}
}
