using Foodo.API.Filters;
using Foodo.API.Models.Request.Profile.Customer;
using Foodo.Application.Commands.Addresses.SetAddressDefault;
using Foodo.Application.Factory.Address;
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
	/// Provides endpoints to manage user addresses such as adding,
	/// deleting, and setting a default address.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This controller handles address-related operations for authenticated users,
	/// with behavior varying based on the user's role.
	/// </para>
	/// <list type="bullet">
	///     <item>
	///         <description>Add a new address to the user's profile</description>
	///     </item>
	///     <item>
	///         <description>Delete an existing address</description>
	///     </item>
	///     <item>
	///         <description>Set a specific address as the default (Customer only)</description>
	///     </item>
	/// </list>
	/// <para>
	/// All endpoints require authentication, and some operations are restricted
	/// to specific user roles.
	/// </para>
	/// </remarks>
	[Route("api/[controller]")]
	[ApiController]
	[EnableRateLimiting("FixedWindowPolicy")]
	public class AdressesController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly IAddressStrategyFactory _factory;

		public AdressesController(IMediator mediator, IAddressStrategyFactory factory)
		{

			_mediator = mediator;
			_factory = factory;
		}


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
			var strategy = _factory.GetCreateAddressStrategy(User);
			var result = await _mediator.Send(strategy);
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
		[ServiceFilter(typeof(ValidateIdFilter))]
		public async Task<IActionResult> DeleteAddress(int id)
		{
			var strategy = _factory.GetDeleteAddressStrategy(User, id);
			var result = await _mediator.Send(strategy);
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
		[ServiceFilter(typeof(ValidateIdFilter))]
		public async Task<IActionResult> SetDefaultAddress(int id)
		{
			var result = await _mediator.Send(new SetAddressDefaultCommand
			{
				CustomerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
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
