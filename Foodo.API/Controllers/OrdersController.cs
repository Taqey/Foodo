using Foodo.API.Filters;
using Foodo.API.Models.Request;
using Foodo.API.Models.Request.Customer;
using Foodo.API.Models.Request.Merchant;
using Foodo.Application.Commands.Orders.CancelOrder;
using Foodo.Application.Commands.Orders.PlaceOrder;
using Foodo.Application.Commands.Orders.UpdateOrder;
using Foodo.Application.Factory.Order;
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
	/// Provides endpoints to manage orders including placing, retrieving,
	/// updating, and cancelling orders.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This controller handles all order-related operations for both customers
	/// and merchants, with behavior determined by the authenticated user's role.
	/// </para>
	/// <list type="bullet">
	///     <item>
	///         <description>Place new orders (Customer only)</description>
	///     </item>
	///     <item>
	///         <description>Retrieve orders and order details</description>
	///     </item>
	///     <item>
	///         <description>Update order status</description>
	///     </item>
	///     <item>
	///         <description>Cancel existing orders</description>
	///     </item>
	/// </list>
	/// <para>
	/// Endpoints are protected using role-based authorization and rate limiting
	/// policies where applicable.
	/// </para>
	/// </remarks>
	[Route("api/[controller]")]
	[ApiController]
	public class OrdersController : ControllerBase
	{
		private readonly IOrderStrategyFactory _factory;
		private readonly IMediator _mediator;

		public OrdersController(IOrderStrategyFactory factory, IMediator mediator)
		{
			_factory = factory;
			_mediator = mediator;
		}

		#region Orders

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
			if (request.Items == null || request.Items.Count == 0)
			{
				Log.Warning("Order attempt failed: No items provided | CustomerId={CustomerId} | TraceId={TraceId}", customerId, HttpContext.TraceIdentifier);
				return BadRequest(new
				{
					message = "Order must contain at least one item",
					traceId = HttpContext.TraceIdentifier
				});
			}
			var result = await _mediator.Send(new PlaceOrderCommand { CustomerId = customerId, Items = request.Items });
			if (!result.IsSuccess)
			{
				Log.Warning("Order placement failed | CustomerId={CustomerId} | Reason={Reason} | TraceId={TraceId}", customerId, result.Message, HttpContext.TraceIdentifier);
				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
			return Ok(new
			{
				message = "Order placed successfully",
				traceId = HttpContext.TraceIdentifier
			});
		}

		/// <summary>
		/// Retrieves a specific order by ID.
		/// </summary>
		/// <param name = "id" > Order ID.</param>
		/// <returns>Order details.</returns>
		/// <response code = "200" > Order retrieved successfully.</response>
		/// <response code = "400" > Failed to retrieve order.</response>
		[HttpGet("{id}")]
		[EnableRateLimiting("TokenBucketPolicy")]
		[ServiceFilter(typeof(ValidateIdFilter))]
		public async Task<IActionResult> GetOrder(int id)
		{
			var strategy = _factory.GetOrderStrategy(User, id);
			var result = await _mediator.Send(strategy);
			if (!result.IsSuccess)
			{
				Log.Warning("Get order by ID failed | OrderId={OrderId} | Reason={Reason} | TraceId={TraceId}", id, result.Message, HttpContext.TraceIdentifier);
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
			return Ok(new
			{
				message = "Order retrieved successfully",
				traceId = HttpContext.TraceIdentifier,
				data = result.Data
			});
		}

		/// <summary>
		/// Retrieves orders of the currently logged-in User.
		/// </summary>
		/// <param name="request">Pagination request.</param>
		/// <returns>List of orders.</returns>
		/// <response code="200">Orders retrieved successfully.</response>
		/// <response code="400">Failed to retrieve orders.</response>
		[HttpGet]
		[EnableRateLimiting("TokenBucketPolicy")]
		public async Task<IActionResult> GetOrders([FromQuery] PaginationRequest request)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var strategy = _factory.GetOrdersStrategy(User, request.PageNumber, request.PageSize);
			var result = await _mediator.Send(strategy);
			if (!result.IsSuccess)
			{
				Log.Warning("Get all orders failed | Reason={Reason} | Page={PageNumber} | PageSize={PageSize} | TraceId={TraceId}", result.Message, request.PageNumber, request.PageSize, HttpContext.TraceIdentifier);
				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
			if (result.Data == null || result.Data.Items.Count == 0)
			{
				Log.Warning("Get all orders returned empty |  Page={PageNumber} | PageSize={PageSize} | TraceId={TraceId}", request.PageNumber, request.PageSize, HttpContext.TraceIdentifier);
				return Ok(new
				{
					message = "No orders found",
					traceId = HttpContext.TraceIdentifier,
					data = result.Data
				});
			}
			return Ok(new
			{
				message = "Orders retrieved successfully",
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
		[ServiceFilter(typeof(ValidateIdFilter))]
		public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateRequest request)
		{
			var result = await _mediator.Send(new UpdateOrderStatusCommand { OrderId = id, OrderState = request.Status });

			if (!result.IsSuccess)
			{
				Log.Warning("Update order status failed | OrderId={OrderId} | Reason={Reason} | TraceId={TraceId}", id, result.Message, HttpContext.TraceIdentifier);

				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
			return Ok(new
			{
				message = "Order status updated successfully",
				traceId = HttpContext.TraceIdentifier
			});
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
		[HttpDelete("{id}")]
		[EnableRateLimiting("SlidingWindowPolicy")]
		[ServiceFilter(typeof(ValidateIdFilter))]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _mediator.Send(new CancelOrderCommand { OrderId = id });

			if (!result.IsSuccess)
			{
				Log.Warning("Order cancellation failed | OrderId={OrderId} | Reason={Reason} | TraceId={TraceId}", id, result.Message, HttpContext.TraceIdentifier);
				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
			return Ok(new
			{
				message = "Order cancelled successfully",
				traceId = HttpContext.TraceIdentifier
			});
		}

		#endregion
	}
}
