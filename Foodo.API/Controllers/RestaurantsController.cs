using Foodo.API.Filters;
using Foodo.API.Models.Request;
using Foodo.Application.Abstraction.Customer;
using Foodo.Application.Abstraction.Merchant;
using Foodo.Application.Abstraction.Profile.MerchantProfile;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Input.Profile.Merchant;
using Foodo.Application.Models.Response;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Foodo.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RestaurantsController : ControllerBase
	{
		private readonly ICustomerService _service;
		private readonly IMerchantService _merchantService;
		private readonly IMerchantAdressService _merchantAdressService;

		public RestaurantsController(ICustomerService service, IMerchantService merchantService, IMerchantAdressService merchantAdressService)
		{
			_service = service;
			_merchantService = merchantService;
			_merchantAdressService = merchantAdressService;
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
		[ServiceFilter(typeof(ValidateIdFilter))]
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

			var result = await _service.ReadShopById(new ItemByIdInput
			{
				ItemId = id
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
		[HttpGet]
		[EnableRateLimiting("TokenBucketPolicy")]
		public async Task<IActionResult> GetShops(
			[FromQuery] PaginationRequest request,
			[FromQuery] int? categoryId = null)
		{
			ApiResponse<PaginationDto<ShopDto>> result;

			if (categoryId.HasValue)
			{
				result = await _service.ReadShopsByCategory(new ShopsPaginationByCategoryInput
				{
					Page = request.PageNumber,
					PageSize = request.PageSize,
					Category = (RestaurantCategory)categoryId
				});
			}
			else
			{
				result = await _service.ReadAllShops(new ProductPaginationInput
				{
					Page = request.PageNumber,
					PageSize = request.PageSize
				});
			}

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
		/// Retrieves the profile details of the authenticated customer.
		/// </summary>
		/// <returns>Customer profile data.</returns>
		/// <response code="200">Profile retrieved successfully.</response>
		/// <response code="400">Failed to retrieve profile.</response>
		[HttpGet("profile")]
		[Authorize(Roles = nameof(UserType.Merchant))]
		[EnableRateLimiting("FixedWindowPolicy")]
		public async Task<IActionResult> GetProfile()
		{
			var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var result = await _merchantAdressService.GetMerchantProfile(new MerchantProfileInput
			{
				MerchantId = UserId
			});

			if (!result.IsSuccess)
			{
				Log.Warning("Failed to retrieve Merchant profile: {Message}", result.Message);
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
		[HttpPost("get-purchased-customers")]
		[EnableRateLimiting("TokenBucketPolicy")]

		public async Task<IActionResult> GetPurchasedCustomers([FromBody] PaginationRequest request)
		{
			var shopId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var result = await _merchantService.ReadAllPurchasedCustomersAsync(new ProductPaginationInput
			{
				Page = request.PageNumber,
				PageSize = request.PageSize,
				UserId = shopId
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
