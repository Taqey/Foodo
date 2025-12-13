using Foodo.API.Models.Request;
using Foodo.API.Models.Request.Customer;
using Foodo.API.Models.Request.Merchant;
using Foodo.Application.Abstraction.Customer;
using Foodo.Application.Abstraction.Merchant;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Input.Merchant;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Security.Claims;

namespace Foodo.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OrdersController : ControllerBase
	{
		private readonly IMerchantService _merchantService;
		private readonly ICustomerService _customerService;

		public OrdersController(IMerchantService merchantService,ICustomerService customerService)
		{
			_merchantService = merchantService;
			_customerService = customerService;
		}
		#region Orders

		/// <summary>
		/// Retrieves all orders of the currently logged-in merchant.
		/// </summary>
		/// <param name="request">Pagination request.</param>
		/// <returns>List of orders.</returns>
		/// <response code="200">Orders retrieved successfully.</response>
		/// <response code="400">Failed to retrieve orders.</response>
		[HttpGet]
		[EnableRateLimiting("TokenBucketPolicy")]
		public async Task<IActionResult> GetAllOrders([FromQuery] PaginationRequest request)
		{
			Log.Information(
				"Get all orders attempt started | Page={Page} | PageSize={PageSize} | TraceId={TraceId}",
				request.PageNumber,
				request.PageSize,
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

				Log.Warning(
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

			// ============================
			// Call Service
			// ============================
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var result = await _merchantService.ReadAllOrdersAsync(new ProductPaginationInput
			{
				Page = request.PageNumber,
				PageSize = request.PageSize,
				UserId = userId
			});

			if (!result.IsSuccess)
			{
				Log.Warning(
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

			if (result.Data == null || result.Data.Items.Count == 0)
			{
				Log.Warning(
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

			Log.Information(
				"Get all orders succeeded | Count={Count} | TraceId={TraceId}",
				result.Data.Items.Count,
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
		/// Retrieves a specific order by ID.
		/// </summary>
		/// <param name="id">Order ID.</param>
		/// <returns>Order details.</returns>
		/// <response code="200">Order retrieved successfully.</response>
		/// <response code="400">Failed to retrieve order.</response>
		[HttpGet("{id}")]
		[EnableRateLimiting("TokenBucketPolicy")]

		public async Task<IActionResult> GetOrderById(int id)
		{
			Log.Information(
				"Get order by ID attempt started | OrderId={OrderId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate Order ID
			// ============================
			if (id <= 0)
			{
				Log.Warning(
					"Invalid OrderId provided | OrderId={OrderId} | TraceId={TraceId}",
					id,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = "Invalid order ID",
					traceId = HttpContext.TraceIdentifier
				});
			}

			// ============================
			// Call Service
			// ============================
			var result = await _merchantService.ReadOrderByIdAsync(id);

			if (!result.IsSuccess)
			{
				Log.Warning(
					"Get order by ID failed | OrderId={OrderId} | Reason={Reason} | TraceId={TraceId}",
					id,
					result.Message,
					HttpContext.TraceIdentifier
				);
				if (result.Message.Contains("found"))
				{
					return NotFound(new
					{
						message = result.Message,
						traceId = HttpContext.TraceIdentifier
					});
				}

				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}

			Log.Information(
				"Get order by ID succeeded | OrderId={OrderId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				message = "Order retrieved successfully",
				traceId = HttpContext.TraceIdentifier,
				data = result.Data
			});
		}

		/// <summary>
		/// Updates the status of an order.
		/// </summary>
		/// <param name="id">Order ID.</param>
		/// <param name="request">New order status.</param>
		/// <returns>Confirmation message.</returns>
		/// <response code="200">Order status updated successfully.</response>
		/// <response code="400">Failed to update order status.</response>
		[HttpPut("{id}/status")]
		[EnableRateLimiting("SlidingWindowPolicy")]

		public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateRequest request)
		{
			Log.Information(
				"Update order status attempt started | OrderId={OrderId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate Order ID
			// ============================
			if (id <= 0)
			{
				Log.Warning(
					"Invalid OrderId provided | OrderId={OrderId} | TraceId={TraceId}",
					id,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = "Invalid order ID",
					traceId = HttpContext.TraceIdentifier
				});
			}

			// ============================
			// Validate Request Body
			// ============================
			if (!ModelState.IsValid)
			{
				var errors = string.Join(" | ",
					ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage));

				Log.Warning(
					"Validation failed while updating order status | OrderId={OrderId} | Errors={Errors} | TraceId={TraceId}",
					id,
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

			// ============================
			// Call Service
			// ============================
			var result = await _merchantService.UpdateOrderStatusAsync(id, new OrderStatusUpdateInput
			{
				Status = request.Status
			});

			if (!result.IsSuccess)
			{
				Log.Warning(
					"Update order status failed | OrderId={OrderId} | Reason={Reason} | TraceId={TraceId}",
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

			Log.Information(
				"Order status updated successfully | OrderId={OrderId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				message = "Order status updated successfully",
				traceId = HttpContext.TraceIdentifier
			});
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
		[HttpPost]
		[EnableRateLimiting("SlidingWindowPolicy")]
		public async Task<IActionResult> Post([FromBody] CreateOrderRequest request)
		{
			var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			Log.Information(
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

				Log.Warning(
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
				Log.Warning(
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
			var result = await _customerService.PlaceOrder(new CreateOrderInput
			{
				CustomerId = customerId,
				Items = request.Items
			});

			if (!result.IsSuccess)
			{
				Log.Warning(
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

			Log.Information(
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


		///// <summary>
		///// Edits an existing order. (Not implemented yet)
		///// </summary>
		///// <param name="id">Order ID to edit.</param>
		///// <param name="value">Updated order data.</param>
		///// <returns>Status indicating whether the update was applied.</returns>
		///// <response code="200">Order edited successfully.</response>
		///// <response code="400">Failed to edit order.</response>
		///// <response code="401">User not authenticated.</response>
		///// <response code="404">Order not found.</response>
		///// <response code="501">Method not implemented.</response>

		//[Authorize(Roles = nameof(UserType.Customer))]
		//[HttpPut("{id}")]
		//[EnableRateLimiting("SlidingWindowPolicy")]

		//public async Task<IActionResult> Put(int id, [FromBody] string value)
		//{
		//	return StatusCode(StatusCodes.Status501NotImplemented);
		//}

		/// <summary>
		/// Cancels a customer's order.
		/// </summary>
		/// <param name="id">Order ID to cancel.</param>
		/// <returns>Cancellation status.</returns>
		/// <response code="200">Order cancelled successfully.</response>
		/// <response code="400">Failed to cancel order.</response>
		/// <response code="401">User not authenticated.</response>
		[Authorize(Roles = nameof(UserType.Customer))]
		[HttpDelete("{id}")]
		[EnableRateLimiting("SlidingWindowPolicy")]
		public async Task<IActionResult> Delete(int id)
		{
			Log.Information(
				"Cancel order attempt started | OrderId={OrderId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			// Validation — ID must be valid
			if (id <= 0)
			{
				Log.Warning(
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

			var result = await _customerService.CancelOrder(new ItemByIdInput { ItemId = id.ToString() });

			if (!result.IsSuccess)
			{
				Log.Warning(
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

			Log.Information(
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
