using Foodo.API.Models.Request;
using Foodo.Application.Queries.Profile.GetMerchantProfile;
using Foodo.Application.Queries.Restaurants.GetRestaurant;
using Foodo.Application.Queries.Restaurants.GetRestaurants;
using Foodo.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Foodo.API.Controllers
{
	/// <summary>
	/// Provides endpoints to manage restaurants (shops) and merchant-related operations.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This controller handles both public and merchant-specific operations including:
	/// </para>
	/// <list type="bullet">
	///     <item>
	///         <description>Browsing restaurants (shops) with pagination and category filtering</description>
	///     </item>
	///     <item>
	///         <description>Retrieving restaurant details by ID</description>
	///     </item>
	///     <item>
	///         <description>Accessing the authenticated merchant profile</description>
	///     </item>
	///     <item>
	///         <description>Retrieving customers who purchased from a merchant</description>
	///     </item>
	/// </list>
	/// <para>
	/// Some endpoints are public, while others require authentication and
	/// role-based authorization for merchants.
	/// </para>
	/// </remarks>
	[Route("api/[controller]")]
	[ApiController]
	public class RestaurantsController : ControllerBase
	{

		private readonly IMediator _mediator;

		public RestaurantsController(IMediator mediator)
		{

			_mediator = mediator;
		}

		#region Shops
		/// <summary>
		/// Retrieves a shop by its ID.
		/// </summary>
		/// <param name="id">Shop ID.</param>
		/// <returns>Shop data if found.</returns>
		/// <response code="200">Shop retrieved successfully.</response>
		/// <response code="400">Invalid shop id.</response>
		/// <response code="404">Shop not found.</response>
		[HttpGet("{id}")]
		[EnableRateLimiting("TokenBucketPolicy")]
		public async Task<IActionResult> GetShopById(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				Log.Warning("GetShopById failed: Invalid ShopId | ShopId={ShopId} | TraceId={TraceId}", id, HttpContext.TraceIdentifier);

				return BadRequest(new
				{
					message = "Invalid shop id.",
					traceId = HttpContext.TraceIdentifier
				});
			}

			var result = await _mediator.Send(new GetRestaurantQuery
			{
				RestaurantId = id
			});

			if (!result.IsSuccess)
			{
				Log.Warning("GetShopById failed: Shop not found | ShopId={ShopId} | TraceId={TraceId}", id, HttpContext.TraceIdentifier);
				return NotFound(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
			return Ok(new
			{
				data = result.Data,
				message = result.Message,
				traceId = HttpContext.TraceIdentifier
			});
		}


		/// <summary>
		/// Retrieves shops with optional category filter, with pagination.
		/// </summary>
		/// <param name="request">Pagination parameters.</param>
		/// <param name="categoryId">Optional category filter.</param>
		/// <returns>Filtered and paginated list of shops.</returns>		
		/// <response code="200">Shops retrieved successfully.</response>
		/// <response code="400">Failed to retrieve shops.</response>
		[HttpGet]
		[EnableRateLimiting("TokenBucketPolicy")]
		public async Task<IActionResult> GetShops(
			[FromQuery] PaginationRequest request,
			[FromQuery] RestaurantCategory? categoryId = null
			)
		{
			var result = await _mediator.Send(new GetRestaurantsQuery { Page = request.PageNumber, PageSize = request.PageSize, Category = categoryId });

			if (!result.IsSuccess)
			{
				Log.Warning("Get shops failed | Reason={Reason} | TraceId={TraceId}", result.Message, HttpContext.TraceIdentifier);
				return BadRequest(new { message = result.Message, traceId = HttpContext.TraceIdentifier });
			}

			if (result.Data == null || result.Data.Items.Count == 0)
			{
				Log.Warning("Get shops returned empty | TraceId={TraceId}", HttpContext.TraceIdentifier);
				return Ok(new { message = "No shops found", traceId = HttpContext.TraceIdentifier, data = result.Data });
			}
			return Ok(new { message = "Shops retrieved successfully", traceId = HttpContext.TraceIdentifier, data = result.Data });
		}

		#endregion

		#region Profile

		/// <summary>
		/// Retrieves the profile details of the authenticated merchant.
		/// </summary>
		/// <returns>merchant profile data.</returns>
		/// <response code="200">Profile retrieved successfully.</response>
		/// <response code="400">Failed to retrieve profile.</response>
		[HttpGet("profile")]
		[Authorize(Roles = nameof(UserType.Merchant))]
		[EnableRateLimiting("FixedWindowPolicy")]
		public async Task<IActionResult> GetProfile()
		{
			var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var result = await _mediator.Send(new GetMerchantProfileQuery
			{
				UserId = UserId
			});

			if (!result.IsSuccess)
			{
				Log.Warning("Failed to retrieve Merchant profile: {Message}", result.Message);
				return BadRequest(result.Message);
			}

			return Ok(result.Data);
		}

		#endregion



	}
}
