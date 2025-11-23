using Foodo.API.Models.Request;
using Foodo.Application.Abstraction.Customer;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Input;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Foodo.API.Controllers
{
	/// <summary>
	/// Provides endpoints for customers to interact with products, shops, and orders.
	/// </summary>
	/// <remarks>
	/// This controller handles all customer-related operations including:
	/// <list type="bullet">
	///     <item>
	///         <description>Reading products (all, by ID, or by category)</description>
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

		public CustomersController(ICustomerService service)
		{
			_service = service;
		}

		#region Products

		/// <summary>
		/// Retrieves all products with pagination.
		/// </summary>
		/// <returns>Returns a paginated list of products.</returns>
		/// <response code="200">Products retrieved successfully.</response>
		/// <response code="400">Failed to retrieve products.</response>
		[HttpPost("get-all-products")]
		public async Task<IActionResult> GetAllproducts(PaginationRequest request)
		{
			var result = await _service.ReadAllProducts(
				new PaginationInput { Page = request.PageNumber, PageSize = request.PageSize });

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Data);
		}

		/// <summary>
		/// Retrieves a product by its ID.
		/// </summary>
		/// <returns>Product data if found.</returns>
		/// <response code="200">Product retrieved successfully.</response>
		/// <response code="400">Failed to retrieve product.</response>
		/// <response code="404">Product not found.</response>
		[HttpGet("get-product-by-id/{id}")]
		public async Task<IActionResult> GetProductbyId(int id)
		{
			var result = await _service.ReadProductById(new ItemByIdInput { ItemId = id.ToString() });

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Data);
		}

		/// <summary>
		/// Retrieves products filtered by category.
		/// </summary>
		/// <returns>Filtered list of products.</returns>
		/// <response code="200">Products retrieved successfully.</response>
		[HttpGet("get-all-products-by-category")]
		public async Task<IActionResult> GetAllproductsByCategory()
		{
			return Ok();
		}

		#endregion

		#region Shops

		/// <summary>
		/// Retrieves all shops with pagination.
		/// </summary>
		/// <returns>Paginated list of shops.</returns>
		/// <response code="200">Shops retrieved successfully.</response>
		/// <response code="400">Failed to retrieve shops.</response>
		[HttpPost("get-all-shops")]
		public async Task<IActionResult> GetAllShops(PaginationRequest request)
		{
			var result = await _service.ReadAllShops(
				new PaginationInput { Page = request.PageNumber, PageSize = request.PageSize });

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Data);
		}

		/// <summary>
		/// Retrieves a shop by ID.
		/// </summary>
		/// <returns>Shop details.</returns>
		/// <response code="200">Shop retrieved successfully.</response>
		/// <response code="400">Failed to retrieve shop.</response>
		/// <response code="404">Shop not found.</response>
		[HttpGet("get-shop-by-id/{id}")]
		public async Task<IActionResult> GetShopById(string id)
		{
			var result = await _service.ReadShopById(new ItemByIdInput { ItemId = id });

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Data);
		}

		/// <summary>
		/// Retrieves shops filtered by category.
		/// </summary>
		/// <returns>Filtered list of shops.</returns>
		/// <response code="200">Shops retrieved successfully.</response>
		[HttpGet("get-all-shops-by-category")]
		public async Task<IActionResult> GetAllShopsByCategory()
		{
			return Ok();
		}

		#endregion

		#region Orders

		/// <summary>
		/// Retrieves all orders for the authenticated customer.
		/// </summary>
		/// <returns>Paginated list of orders.</returns>
		/// <response code="200">Orders retrieved successfully.</response>
		/// <response code="400">Failed to retrieve orders.</response>
		/// <response code="401">User not authenticated.</response>
		[Authorize(Roles = nameof(UserType.Customer))]
		[HttpPost("get-all-orders")]
		public async Task<IActionResult> GetAllOrders(PaginationRequest request)
		{
			var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var result = await _service.ReadAllOrders(
				new PaginationInput { Page = request.PageNumber, PageSize = request.PageSize, FilterBy = id });

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Data);
		}

		/// <summary>
		/// Retrieves a specific order by its ID.
		/// </summary>
		/// <returns>Order data.</returns>
		/// <response code="200">Order retrieved successfully.</response>
		/// <response code="400">Failed to retrieve order.</response>
		/// <response code="404">Order not found.</response>
		/// <response code="401">User not authenticated.</response>
		[Authorize(Roles = nameof(UserType.Customer))]
		[HttpGet("get-order-by-id/{id}")]
		public async Task<IActionResult> GetOrdersById(string id)
		{
			var result = await _service.ReadOrderById(new ItemByIdInput { ItemId = id });

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Data);
		}

		/// <summary>
		/// Places a new order.
		/// </summary>
		/// <returns>Order summary.</returns>
		/// <response code="200">Order placed successfully.</response>
		/// <response code="400">Failed to place order.</response>
		/// <response code="401">User not authenticated.</response>
		[Authorize(Roles = nameof(UserType.Customer))]
		[HttpPost("place-order")]
		public async Task<IActionResult> Post([FromBody] CreateOrderRequest request)
		{
			var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var result = await _service.PlaceOrder(new CreateOrderInput
			{
				CustomerId = id,
				Items = request.Items
			});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result);
		}

		/// <summary>
		/// Edits an existing order.
		/// </summary>
		/// <returns>Status of update.</returns>
		/// <response code="200">Order edited successfully.</response>
		/// <response code="400">Failed to edit order.</response>
		/// <response code="401">User not authenticated.</response>
		[Authorize(Roles = nameof(UserType.Customer))]
		[HttpPut("edit-order/{id}")]
		public async Task<IActionResult> Put(int id, [FromBody] string value)
		{
			return Ok("Not implemented yet.");
		}

		/// <summary>
		/// Cancels a customer's order.
		/// </summary>
		/// <returns>Cancellation status.</returns>
		/// <response code="200">Order cancelled successfully.</response>
		/// <response code="400">Failed to cancel order.</response>
		/// <response code="401">User not authenticated.</response>
		[Authorize(Roles = nameof(UserType.Customer))]
		[HttpDelete("cancel-order/{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _service.CancelOrder(new ItemByIdInput { ItemId = id.ToString() });

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}

		#endregion
	}
}
