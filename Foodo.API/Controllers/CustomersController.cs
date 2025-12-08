using Foodo.API.Models.Request;
using Foodo.API.Models.Request.Customer;
using Foodo.Application.Abstraction.Customer;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
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
		private readonly ILogger<CustomersController> _logger;

		public CustomersController(ICustomerService service, ILogger<CustomersController> logger)
		{
			_service = service;
			_logger = logger;
		}

		#region Products

		/// <summary>
		/// Retrieves all products with pagination.
		/// </summary>
		/// <param name="request">Pagination parameters.</param>
		/// <returns>Paginated list of products.</returns>
		/// <response code="200">Products retrieved successfully.</response>
		/// <response code="400">Failed to retrieve products.</response>
		[HttpPost("get-all-products")]
		[EnableRateLimiting("TokenBucketPolicy")]
		public async Task<IActionResult> GetAllProducts([FromBody] PaginationRequest request)
		{
			_logger.LogInformation("Get all products attempt started | Page={Page} | PageSize={PageSize} | TraceId={TraceId}", request.PageNumber, request.PageSize, HttpContext.TraceIdentifier);
			if (!ModelState.IsValid)
			{
				var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

				_logger.LogWarning("Validation failed | Errors={Errors} | TraceId={TraceId}", errors, HttpContext.TraceIdentifier);

				return BadRequest(new
				{
					message = "Invalid request data",
					errors,
					traceId = HttpContext.TraceIdentifier
				});
			}
			var result = await _service.ReadAllProducts(new ProductPaginationInput
			{
				Page = request.PageNumber,
				PageSize = request.PageSize
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning("Get all products failed | Reason={Reason} | TraceId={TraceId}", result.Message, HttpContext.TraceIdentifier);

				return BadRequest(new
				{
					message = "Retrieving failed",
					traceId = HttpContext.TraceIdentifier,
				});
			}

			if (result.Data == null || result.Data.Items.Count == 0)
			{
				_logger.LogWarning("Get all products returned empty | TraceId={TraceId}", HttpContext.TraceIdentifier);

				return Ok(new
				{
					message = "No products found ",
					traceId = HttpContext.TraceIdentifier,
					data = result.Data
				});
			}
			_logger.LogInformation("Get all products succeeded | Count={Count} | TraceId={TraceId}", result.Data?.Items?.Count ?? 0, HttpContext.TraceIdentifier);

			return Ok(new
			{
				message = "Products retrieved successfully",
				traceId = HttpContext.TraceIdentifier,
				data = result.Data
			});
		}


		/// <summary>
		/// Retrieves a product by its ID.
		/// </summary>
		/// <param name="id">Product ID.</param>
		/// <returns>Product data if found.</returns>
		/// <response code="200">Product retrieved successfully.</response>
		/// <response code="400">Failed to retrieve product.</response>
		/// <response code="404">Product not found.</response>
		[HttpGet("get-product-by-id/{id}")]
		[EnableRateLimiting("TokenBucketPolicy")]
		public async Task<IActionResult> GetProductbyId(int id)
		{
			_logger.LogInformation(
				"GetProductById attempt started | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			if (id <= 0)
			{
				_logger.LogWarning(
					"Get all Products By Id failed: Invalid product id | ProductId={ProductId} | TraceId={TraceId}",
					id,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = "Invalid product id.",
					traceId = HttpContext.TraceIdentifier
				});
			}

			var result = await _service.ReadProductById(new ItemByIdInput
			{
				ItemId = id.ToString()
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Get all Products By Id  failed: Product not found | ProductId={ProductId} | TraceId={TraceId}",
					id,
					HttpContext.TraceIdentifier
				);

				return NotFound(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}

			_logger.LogInformation(
				"Get all Products By Id  succeeded | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			return Ok(result.Data);
		}


		/// <summary>
		/// Retrieves products filtered by category.
		/// </summary>
		/// <param name="request">Pagination and category parameters.</param>
		/// <returns>Filtered list of products.</returns>
		/// <response code="200">Products retrieved successfully.</response>
		/// <response code="400">Failed to retrieve products.</response>
		[HttpPost("get-all-products-by-category")]
		[EnableRateLimiting("TokenBucketPolicy")]
		public async Task<IActionResult> GetAllproductsByCategory(ProductCategoryPaginationRequest request)
		{
			_logger.LogInformation(
				"Get all Products By Category attempt started | Category={Category} | Page={Page} | PageSize={PageSize} | TraceId={TraceId}",
				request.Category,
				request.PageNumber,
				request.PageSize,
				HttpContext.TraceIdentifier
			);

			if (!ModelState.IsValid)
			{
				var errors = string.Join(" | ", ModelState.Values
					.SelectMany(v => v.Errors)
					.Select(e => e.ErrorMessage));

				_logger.LogWarning(
					"Get all Products By Category failed: Invalid model | Errors={Errors} | TraceId={TraceId}",
					errors,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = errors,
					traceId = HttpContext.TraceIdentifier
				});
			}

			var result = await _service.ReadProductsByCategory(new ProductPaginationByCategoryInput
			{
				Page = request.PageNumber,
				PageSize = request.PageSize,
				Category = request.Category
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Get all Products By Category failed | Category={Category} | Reason={Reason} | TraceId={TraceId}",
					request.Category,
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
				_logger.LogWarning("Get all Products By Category returned empty | TraceId={TraceId}", HttpContext.TraceIdentifier);

				return Ok(new
				{
					message = "No products found ",
					traceId = HttpContext.TraceIdentifier,
					data = result.Data
				});
			}
			_logger.LogInformation(
				"Get All Products By Category succeeded | Category={Category} | TraceId={TraceId}",
				request.Category,
				HttpContext.TraceIdentifier
			);

			return Ok(result.Data);
		}


		/// <summary>
		/// Retrieves all products belonging to a specific restaurant (merchant), with pagination.
		/// </summary>
		/// <param name="request">Pagination and merchant ID.</param>
		/// <returns>Paginated list of restaurant products.</returns>
		/// <response code="200">Products retrieved successfully.</response>
		/// <response code="400">Failed to retrieve products.</response>
		[HttpPost("get-all-products-by-restaurant")]
		[EnableRateLimiting("TokenBucketPolicy")]
		public async Task<IActionResult> GetAllProductsByRestaurant([FromBody] ProductPaginationByShopRequest request)
		{
			_logger.LogInformation(
				"Get all products By Restaurant attempt started | RestaurantId={MerchantId} | Page={Page} | PageSize={Page} | TraceId={TraceId}",
				request.MerchantId,
				request.PageNumber,
				request.PageSize,
				HttpContext.TraceIdentifier

			);

			if (!ModelState.IsValid)
			{
				var errors = string.Join(" | ",
					ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage));

				_logger.LogWarning(
					"Validation failed | Errors={Errors} | TraceId={TraceId}",
					errors,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = "Invalid request data",
					errors,
					traceId = HttpContext.TraceIdentifier
				});
			}

			var result = await _service.ReadProductsByShop(new ProductPaginationByShopInput
			{
				Page = request.PageNumber,
				PageSize = request.PageSize,
				MerchantId = request.MerchantId
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Get all products By Restaurant failed | RestaurantId={MerchantId} | Reason={Reason} | TraceId={TraceId}",
					request.MerchantId,
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
				_logger.LogWarning("Get all products By Restaurant returned empty | TraceId={TraceId}", HttpContext.TraceIdentifier);

				return Ok(new
				{
					message = "No products found ",
					traceId = HttpContext.TraceIdentifier,
					data = result.Data
				});
			}
			_logger.LogInformation(
				"Get all products By Restaurant succeeded | RestaurantId={MerchantId} | TraceId={TraceId}",
				request.MerchantId,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				message = "Products retrieved successfully",
				traceId = HttpContext.TraceIdentifier,
				data = result.Data
			});
		}



		#endregion

		#region Shops

		/// <summary>
		/// Retrieves all shops with pagination.
		/// </summary>
		/// <param name="request">Pagination parameters.</param>
		/// <returns>Paginated list of shops.</returns>
		/// <response code="200">Shops retrieved successfully.</response>
		/// <response code="400">Failed to retrieve shops.</response>
		[HttpPost("get-all-shops")]
		[EnableRateLimiting("TokenBucketPolicy")]
		public async Task<IActionResult> GetAllShops([FromBody] PaginationRequest request)
		{
			_logger.LogInformation(
				"Get all shops attempt started | Page={Page} | PageSize={PageSize} | TraceId={TraceId}",
				request.PageNumber,
				request.PageSize,
				HttpContext.TraceIdentifier
			);

			if (!ModelState.IsValid)
			{
				var errors = string.Join(" | ",
					ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage));

				_logger.LogWarning(
					"Validation failed | Errors={Errors} | TraceId={TraceId}",
					errors,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = "Invalid request data",
					errors,
					traceId = HttpContext.TraceIdentifier
				});
			}

			var result = await _service.ReadAllShops(new ProductPaginationInput
			{
				Page = request.PageNumber,
				PageSize = request.PageSize
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Get all shops failed | Reason={Reason} | TraceId={TraceId}",
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
				_logger.LogWarning(
					"Get all shops returned empty | TraceId={TraceId}",
					HttpContext.TraceIdentifier
				);

				return Ok(new
				{
					message = "No shops found",
					traceId = HttpContext.TraceIdentifier,
					data = result.Data
				});
			}

			_logger.LogInformation(
				"Get all shops succeeded | Count={Count} | TraceId={TraceId}",
				result.Data?.Items?.Count ?? 0,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				message = "Shops retrieved successfully",
				traceId = HttpContext.TraceIdentifier,
				data = result.Data
			});
		}


		/// <summary>
		/// Retrieves a shop by its ID.
		/// </summary>
		/// <param name="id">Shop ID.</param>
		/// <returns>Shop data if found.</returns>
		/// <response code="200">Shop retrieved successfully.</response>
		/// <response code="400">Invalid shop id.</response>
		/// <response code="404">Shop not found.</response>
		[HttpGet("get-shop-by-id/{id}")]
		[EnableRateLimiting("TokenBucketPolicy")]
		public async Task<IActionResult> GetShopById(string id)
		{
			_logger.LogInformation(
				"GetShopById attempt started | ShopId={ShopId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			if (string.IsNullOrWhiteSpace(id))
			{
				_logger.LogWarning(
					"GetShopById failed: Invalid ShopId | ShopId={ShopId} | TraceId={TraceId}",
					id,
					HttpContext.TraceIdentifier
				);

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
				_logger.LogWarning(
					"GetShopById failed: Shop not found | ShopId={ShopId} | TraceId={TraceId}",
					id,
					HttpContext.TraceIdentifier
				);

				return NotFound(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}

			_logger.LogInformation(
				"GetShopById succeeded | ShopId={ShopId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				data = result.Data,
				message = result.Message,
				traceId = HttpContext.TraceIdentifier
			});
		}



		/// <summary>
		/// Retrieves shops filtered by category.
		/// </summary>
		/// <param name="request">Pagination and category parameters.</param>
		/// <returns>Filtered list of shops.</returns>
		/// <response code="200">Shops retrieved successfully.</response>
		/// <response code="400">Failed to retrieve shops.</response>
		[HttpPost("get-all-shops-by-category")]
		[EnableRateLimiting("TokenBucketPolicy")]

		public async Task<IActionResult> GetAllShopsByCategory([FromBody] ShopCategoryPaginationRequest request)
		{
			_logger.LogInformation(
				"Get all shops by category attempt started | Category={Category} | Page={Page} | PageSize={PageSize} | TraceId={TraceId}",
				request.Category,
				request.PageNumber,
				request.PageSize,
				HttpContext.TraceIdentifier
			);

			// ⛔ Invalid ModelState
			if (!ModelState.IsValid)
			{
				var errors = string.Join(" | ",
					ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage));

				_logger.LogWarning(
					"Get all shops by category failed: Invalid model | Errors={Errors} | TraceId={TraceId}",
					errors,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = errors,
					traceId = HttpContext.TraceIdentifier
				});
			}

			// 📌 Service Call
			var result = await _service.ReadShopsByCategory(new ShopsPaginationByCategoryInput
			{
				Page = request.PageNumber,
				PageSize = request.PageSize,
				Category = request.Category
			});

			// ⛔ Service Error
			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Get all shops by category failed | Category={Category} | Reason={Reason} | TraceId={TraceId}",
					request.Category,
					result.Message,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}

			// ⛔ No Shops Found
			if (result.Data == null || result.Data.Items.Count == 0)
			{
				_logger.LogWarning(
					"Get all shops by category returned empty | Category={Category} | TraceId={TraceId}",
					request.Category,
					HttpContext.TraceIdentifier
				);

				return Ok(new
				{
					message = "No shops found",
					traceId = HttpContext.TraceIdentifier,
					data = result.Data
				});
			}

			// ✅ Success
			_logger.LogInformation(
				"Get all shops by category succeeded | Category={Category} | Count={Count} | TraceId={TraceId}",
				request.Category,
				result.Data?.Items?.Count ?? 0,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				message = "Shops retrieved successfully",
				traceId = HttpContext.TraceIdentifier,
				data = result.Data
			});
		}


		#endregion

		#region Orders

		/// <summary>
		/// Retrieves all orders for the authenticated customer.
		/// </summary>
		/// <param name="request">Pagination parameters.</param>
		/// <returns>Paginated list of orders.</returns>
		/// <response code="200">Orders retrieved successfully.</response>
		/// <response code="400">Failed to retrieve orders.</response>
		/// <response code="401">User not authenticated.</response>
		[Authorize(Roles = nameof(UserType.Customer))]
		[HttpPost("get-all-orders")]
		[EnableRateLimiting("TokenBucketPolicy")]
		public async Task<IActionResult> GetAllOrders([FromBody] PaginationRequest request)
		{
			_logger.LogInformation(
				"Get all orders attempt started | Page={Page} | PageSize={PageSize} | TraceId={TraceId}",
				request.PageNumber,
				request.PageSize,
				HttpContext.TraceIdentifier
			);

			// ⛔ ModelState invalid
			if (!ModelState.IsValid)
			{
				var errors = string.Join(" | ",
					ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage));

				_logger.LogWarning(
					"Get all orders failed: Invalid model | Errors={Errors} | TraceId={TraceId}",
					errors,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = "Invalid request data",
					errors,
					traceId = HttpContext.TraceIdentifier
				});
			}

			// ⛔ Missing User Id
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrWhiteSpace(userId))
			{
				_logger.LogWarning(
					"Get all orders failed: Unauthorized user | TraceId={TraceId}",
					HttpContext.TraceIdentifier
				);

				return Unauthorized(new
				{
					message = "User not authenticated",
					traceId = HttpContext.TraceIdentifier
				});
			}

			// 📌 Service call
			var result = await _service.ReadAllOrders(new ProductPaginationInput
			{
				Page = request.PageNumber,
				PageSize = request.PageSize,
				UserId = userId
			});

			// ⛔ Service failed
			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Get all orders failed | Reason={Reason} | TraceId={TraceId}",
					result.Message,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}

			// ⛔ No orders found
			if (result.Data == null || result.Data.Items.Count == 0)
			{
				_logger.LogWarning(
					"Get all orders returned empty | TraceId={TraceId}",
					HttpContext.TraceIdentifier
				);

				return Ok(new
				{
					message = "No orders found",
					traceId = HttpContext.TraceIdentifier,
					data = result.Data
				});
			}

			// ✅ Success
			_logger.LogInformation(
				"Get all orders succeeded | Count={Count} | TraceId={TraceId}",
				result.Data?.Items?.Count ?? 0,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				message = "Orders retrieved successfully",
				traceId = HttpContext.TraceIdentifier,
				data = result.Data
			});
		}


		/// <summary>
		/// Retrieves a specific order by its ID.
		/// </summary>
		/// <param name="id">Order ID.</param>
		/// <returns>Order data.</returns>
		/// <response code="200">Order retrieved successfully.</response>
		/// <response code="400">Invalid order ID or failed to retrieve order.</response>
		/// <response code="404">Order not found.</response>
		/// <response code="401">User not authenticated.</response>
		[Authorize(Roles = nameof(UserType.Customer))]
		[HttpGet("get-order-by-id/{id}")]
		[EnableRateLimiting("TokenBucketPolicy")]
		public async Task<IActionResult> GetOrdersById(string id)
		{
			_logger.LogInformation(
				"Get order by id attempt started | OrderId={OrderId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			if (string.IsNullOrWhiteSpace(id))
			{
				_logger.LogWarning(
					"Invalid OrderId provided | TraceId={TraceId}",
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = "OrderId is required",
					traceId = HttpContext.TraceIdentifier
				});
			}

			var result = await _service.ReadOrderById(new ItemByIdInput { ItemId = id });

			if (!result.IsSuccess)
			{
				if (result.Message.Contains("found", StringComparison.OrdinalIgnoreCase))
				{
					_logger.LogWarning(
						"Order not found | OrderId={OrderId} | TraceId={TraceId}",
						id,
						HttpContext.TraceIdentifier
					);

					return NotFound(new
					{
						message = "Order not found",
						traceId = HttpContext.TraceIdentifier
					});
				}

				_logger.LogError(
					"Failed to retrieve order | OrderId={OrderId} | Error={Error} | TraceId={TraceId}",
					id,
					result.Message,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}

			_logger.LogInformation(
				"Order retrieved successfully | OrderId={OrderId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			return Ok(result.Data);
		}


		/// <summary>
		/// Places a new order.
		/// </summary>
		/// <param name="request">Order creation data.</param>
		/// <returns>Success or failure message.</returns>
		/// <response code="200">Order placed successfully.</response>
		/// <response code="400">Failed to place order.</response>
		/// <response code="401">User not authenticated.</response>
		[Authorize(Roles = nameof(UserType.Customer))]
		[HttpPost("place-order")]
		[EnableRateLimiting("SlidingWindowPolicy")]
		public async Task<IActionResult> Post([FromBody] CreateOrderRequest request)
		{
			var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			_logger.LogInformation(
				"Place order attempt started | CustomerId={CustomerId} | ItemsCount={ItemsCount} | TraceId={TraceId}",
				customerId,
				request?.Items?.Count,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate Request
			// ============================
			if (!ModelState.IsValid)
			{
				var errors = string.Join(" | ",
					ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage));

				_logger.LogWarning(
					"Validation failed while placing order | CustomerId={CustomerId} | Errors={Errors} | TraceId={TraceId}",
					customerId,
					errors,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = "Invalid request data",
					errors,
					traceId = HttpContext.TraceIdentifier
				});
			}

			if (request.Items == null || request.Items.Count == 0)
			{
				_logger.LogWarning(
					"Order attempt failed: No items provided | CustomerId={CustomerId} | TraceId={TraceId}",
					customerId,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = "Order must contain at least one item",
					traceId = HttpContext.TraceIdentifier
				});
			}

			// ============================
			// Call Service
			// ============================
			var result = await _service.PlaceOrder(new CreateOrderInput
			{
				CustomerId = customerId,
				Items = request.Items
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Order placement failed | CustomerId={CustomerId} | Reason={Reason} | TraceId={TraceId}",
					customerId,
					result.Message,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}

			_logger.LogInformation(
				"Order placed successfully | CustomerId={CustomerId} | TraceId={TraceId}",
				customerId,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				message = "Order placed successfully",
				traceId = HttpContext.TraceIdentifier
			});
		}


		/// <summary>
		/// Edits an existing order. (Not implemented yet)
		/// </summary>
		/// <param name="id">Order ID to edit.</param>
		/// <param name="value">Updated order data.</param>
		/// <returns>Status indicating whether the update was applied.</returns>
		/// <response code="200">Order edited successfully.</response>
		/// <response code="400">Failed to edit order.</response>
		/// <response code="401">User not authenticated.</response>
		/// <response code="404">Order not found.</response>
		/// <response code="501">Method not implemented.</response>

		[Authorize(Roles = nameof(UserType.Customer))]
		[HttpPut("edit-order/{id}")]
		[EnableRateLimiting("SlidingWindowPolicy")]

		public async Task<IActionResult> Put(int id, [FromBody] string value)
		{
			return StatusCode(StatusCodes.Status501NotImplemented);
		}

		/// <summary>
		/// Cancels a customer's order.
		/// </summary>
		/// <param name="id">Order ID to cancel.</param>
		/// <returns>Cancellation status.</returns>
		/// <response code="200">Order cancelled successfully.</response>
		/// <response code="400">Failed to cancel order.</response>
		/// <response code="401">User not authenticated.</response>
		[Authorize(Roles = nameof(UserType.Customer))]
		[HttpDelete("cancel-order/{id}")]
		[EnableRateLimiting("SlidingWindowPolicy")]
		public async Task<IActionResult> Delete(int id)
		{
			_logger.LogInformation(
				"Cancel order attempt started | OrderId={OrderId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			// Validation — ID must be valid
			if (id <= 0)
			{
				_logger.LogWarning(
					"Invalid order ID provided | OrderId={OrderId} | TraceId={TraceId}",
					id,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = "Invalid order ID",
					traceId = HttpContext.TraceIdentifier
				});
			}

			var result = await _service.CancelOrder(new ItemByIdInput { ItemId = id.ToString() });

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Order cancellation failed | OrderId={OrderId} | Reason={Reason} | TraceId={TraceId}",
					id,
					result.Message,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}

			_logger.LogInformation(
				"Order cancelled successfully | OrderId={OrderId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				message = "Order cancelled successfully",
				traceId = HttpContext.TraceIdentifier
			});
		}


		#endregion
	}
}
