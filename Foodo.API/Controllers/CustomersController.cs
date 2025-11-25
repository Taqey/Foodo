using Foodo.API.Models.Request;
using Foodo.API.Models.Request.Customer;
using Foodo.Application.Abstraction.Customer;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
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

		public CustomersController(ICustomerService service)
		{
			_service = service;
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
		public async Task<IActionResult> GetAllproducts(PaginationRequest request)
		{
			var result = await _service.ReadAllProducts(
				new ProductPaginationInput { Page = request.PageNumber, PageSize = request.PageSize });

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Data);
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
		/// <param name="request">Pagination and category parameters.</param>
		/// <returns>Filtered list of products.</returns>
		/// <response code="200">Products retrieved successfully.</response>
		/// <response code="400">Failed to retrieve products.</response>
		[HttpPost("get-all-products-by-category")]
		public async Task<IActionResult> GetAllproductsByCategory(ProductCategoryPaginationRequest request)
		{
			var result = await _service.ReadProductsByCategory(
				new ProductPaginationByCategoryInput { Page = request.PageNumber, PageSize = request.PageSize, Category = request.Category });

			if (!result.IsSuccess)
				return BadRequest(result.Message);

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
		public async Task<IActionResult> GetAllProductsByRestaurant(ProductPaginationByShopRequest request)
		{
			var result = await _service.ReadProductsByShop(
				new ProductPaginationByShopInput
				{
					Page = request.Page,
					PageSize = request.PageSize,
					MerchantId = request.MerchantId
				});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Data);
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
		public async Task<IActionResult> GetAllShops(PaginationRequest request)
		{
			var result = await _service.ReadAllShops(
				new ProductPaginationInput { Page = request.PageNumber, PageSize = request.PageSize });

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Data);
		}

		/// <summary>
		/// Retrieves a shop by ID.
		/// </summary>
		/// <param name="id">Shop ID.</param>
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
		/// <param name="request">Pagination and category parameters.</param>
		/// <returns>Filtered list of shops.</returns>
		/// <response code="200">Shops retrieved successfully.</response>
		/// <response code="400">Failed to retrieve shops.</response>
		[HttpPost("get-all-shops-by-category")]
		public async Task<IActionResult> GetAllShopsByCategory(ShopCategoryPaginationRequest request)
		{
			var result = await _service.ReadShopsByCategory(
				new ShopsPaginationByCategoryInput { Page = request.PageNumber, PageSize = request.PageSize, Category = request.Category });

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Data);
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
		public async Task<IActionResult> GetAllOrders(PaginationRequest request)
		{
			var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var result = await _service.ReadAllOrders(
				new ProductPaginationInput { Page = request.PageNumber, PageSize = request.PageSize, UserId = id });

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Data);
		}

		/// <summary>
		/// Retrieves a specific order by its ID.
		/// </summary>
		/// <param name="id">Order ID.</param>
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
		/// <param name="request">Order creation data.</param>
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
		/// Edits an existing order. (Not implemented yet)
		/// </summary>
		/// <param name="id">Order ID to edit.</param>
		/// <param name="value">Updated order data.</param>
		/// <returns>Status indicating whether the update was applied.</returns>
		/// <response code="200">Order edited successfully.</response>
		/// <response code="400">Failed to edit order.</response>
		/// <response code="401">User not authenticated.</response>
		/// <response code="404">Order not found.</response>
		[Authorize(Roles = nameof(UserType.Customer))]
		[HttpPut("edit-order/{id}")]
		public async Task<IActionResult> Put(int id, [FromBody] string value)
		{
			return Ok("Not implemented yet.");
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
