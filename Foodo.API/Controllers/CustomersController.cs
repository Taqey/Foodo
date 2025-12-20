using Foodo.API.Models.Request;
using Foodo.Application.Queries.Customers;
using Foodo.Application.Queries.Profile.GetCustomerProfile;
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
	/// Provides endpoints to retrieve customer profile information.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This controller is responsible for customer profile-related operations,
	/// allowing authenticated customers to access their personal profile data.
	/// </para>
	/// <para>
	/// All endpoints are protected and require Bearer authentication with
	/// the Customer role.
	/// </para>
	/// </remarks>
	[Route("api/[controller]")]
	[ApiController]
	public class CustomersController : ControllerBase
	{
		private readonly IMediator _mediator;

		public CustomersController(IMediator mediator)
		{
			_mediator = mediator;
		}

		#region Get Profile

		/// <summary>
		/// Retrieves the profile details of the authenticated customer.
		/// </summary>
		/// <returns>Customer profile data.</returns>
		/// <response code="200">Profile retrieved successfully.</response>
		/// <response code="400">Failed to retrieve profile.</response>
		[HttpGet("profile")]
		[Authorize(Roles = nameof(UserType.Customer))]
		[EnableRateLimiting("FixedWindowPolicy")]
		public async Task<IActionResult> GetProfile()
		{
			var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var result = await _mediator.Send(new GetCustomerProfileQuery
			{
				UserId = UserId
			});

			if (!result.IsSuccess)
			{
				Log.Warning("Failed to retrieve customer profile: {Message}", result.Message);
				return BadRequest(result.Message);
			}

			return Ok(result.Data);
		}

		#endregion

		#region Purchased Customers

		/// <summary>
		/// Retrieves all customers who purchased from the logged-in merchant.
		/// </summary>
		/// <param name="request">Pagination request.</param>
		/// <returns>List of purchased customers.</returns>
		/// <response code="200">Customers retrieved successfully.</response>
		/// <response code="400">Failed to retrieve customers.</response>
		[HttpGet()]
		[EnableRateLimiting("TokenBucketPolicy")]
		[Authorize(Roles = nameof(UserType.Merchant))]
		public async Task<IActionResult> GetPurchasedCustomers([FromQuery] PaginationRequest request)
		{
			var shopId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var result = await _mediator.Send(new GetCustomersQuery
			{
				Page = request.PageNumber,
				PageSize = request.PageSize,
				RestaurantId = shopId
			});

			if (!result.IsSuccess)
			{
				Log.Warning(
						"Failed to retrieve purchased customers | ShopId={ShopId} | Reason={Reason} | TraceId={TraceId}",
						shopId,
						result.Message,
						HttpContext.TraceIdentifier
					);

				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
			if (result.Data == null || result.Data.Items.Count == 0)
			{

				return Ok(new
				{
					message = "No Customers found",
					traceId = HttpContext.TraceIdentifier,
					data = result.Data
				});
			}

			return Ok(new
			{
				message = "Customers retrieved successfully",
				traceId = HttpContext.TraceIdentifier,
				data = result.Data
			});
		}


		#endregion
	}
}
