using Foodo.API.Models.Request;
using Foodo.Application.Abstraction.Merchant;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Foodo.API.Controllers
{
	/// <summary>
	/// Provides endpoints to manage products and orders for merchants, including
	/// creating, updating, deleting products, managing product attributes, 
	/// and handling orders.
	/// </summary>
	/// <remarks>
	/// <para>This controller handles all merchant-related operations:</para>
	/// <list type="bullet">
	///     <item><description>Retrieve, create, update, and delete products</description></item>
	///     <item><description>Add or remove product attributes</description></item>
	///     <item><description>Retrieve orders and update order status</description></item>
	/// </list>
	/// <para>
	/// All endpoints require Bearer authentication and "Merchant" role.
	/// All input models are received via <c>[FromBody]</c>.
	/// </para>
	/// </remarks>
	[Authorize(Roles = nameof(UserType.Merchant))]
	[Route("api/[controller]")]
	[ApiController]
	public class MerchantsController : ControllerBase
	{
		private readonly IProductService _productService;

		public MerchantsController(IProductService productService)
		{
			_productService = productService;
		}

		#region Products

		/// <summary>
		/// Retrieves all products for the currently logged-in merchant.
		/// </summary>
		/// <returns>Returns 200 OK with a list of products or 400 Bad Request on failure.</returns>
		/// <response code="200">Products retrieved successfully.</response>
		/// <response code="400">Failed to retrieve products.</response>
		[HttpGet("get-all-products")]
		public async Task<ActionResult> GetAllProducts()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var result = await _productService.ReadAllProductsAsync(userId);

			if (!result.IsSuccess)
				return BadRequest(result);

			return Ok(result.Data);
		}

		/// <summary>
		/// Retrieves a product by its ID.
		/// </summary>
		/// <param name="id">ID of the product.</param>
		/// <returns>Returns 200 OK with product data or 400 Bad Request on failure.</returns>
		/// <response code="200">Product retrieved successfully.</response>
		/// <response code="400">Failed to retrieve product.</response>
		[HttpGet("get-product-by-id/{id}")]
		public async Task<IActionResult> GetProductById(int id)
		{
			var result = await _productService.ReadProductByIdAsync(id);
			if (!result.IsSuccess)
				return BadRequest(result);

			return Ok(result.Data);
		}

		/// <summary>
		/// Creates a new product for the logged-in merchant.
		/// </summary>
		/// <param name="request">Product creation request containing name, description, price, and attributes.</param>
		/// <returns>Returns 200 OK if successful or 400 Bad Request on failure.</returns>
		/// <response code="200">Product created successfully.</response>
		/// <response code="400">Failed to create product.</response>
		[HttpPost("create-product")]
		public async Task<IActionResult> CreateProduct([FromBody] ProductRequest request)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var result = await _productService.CreateProductAsync(new ProductInput
			{
				Id = userId,
				ProductName = request.ProductName,
				ProductDescription = request.ProductDescription,
				Price = request.Price,
				Attributes = request.Attributes
			});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}

		/// <summary>
		/// Updates an existing product by its ID.
		/// </summary>
		/// <param name="id">ID of the product to update.</param>
		/// <param name="request">Updated product details.</param>
		/// <returns>Returns 200 OK if successful or 400 Bad Request on failure.</returns>
		/// <response code="200">Product updated successfully.</response>
		/// <response code="400">Failed to update product.</response>
		[HttpPut("update-product/{id}")]
		public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductRequest request)
		{
			var result = await _productService.UpdateProductAsync(id, new ProductInput
			{
				ProductName = request.ProductName,
				ProductDescription = request.ProductDescription,
				Price = request.Price,
				Attributes = request.Attributes
			});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}

		/// <summary>
		/// Deletes a product by its ID.
		/// </summary>
		/// <param name="id">ID of the product to delete.</param>
		/// <returns>Returns 200 OK if deleted or 400 Bad Request on failure.</returns>
		/// <response code="200">Product deleted successfully.</response>
		/// <response code="400">Failed to delete product.</response>
		[HttpDelete("delete-product/{id}")]
		public async Task<IActionResult> DeleteProduct(int id)
		{
			var result = await _productService.DeleteProductAsync(id);
			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}

		#endregion

		#region Product Attributes

		/// <summary>
		/// Adds attributes to an existing product.
		/// </summary>
		/// <param name="id">ID of the product.</param>
		/// <param name="request">Attributes to add.</param>
		/// <returns>Returns 200 OK if attributes are added or 400 Bad Request on failure.</returns>
		/// <response code="200">Attributes added successfully.</response>
		/// <response code="400">Failed to add attributes.</response>
		[HttpPut("add-attribute")]
		public async Task<IActionResult> AddAttribute(int id, [FromBody] AttributeCreateRequest request)
		{
			var result = await _productService.AddProductAttributeAsync(id, new AttributeCreateInput
			{
				Attributes = request.Attributes
			});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}

		/// <summary>
		/// Removes attributes from an existing product.
		/// </summary>
		/// <param name="id">ID of the product.</param>
		/// <param name="request">Attributes to remove.</param>
		/// <returns>Returns 200 OK if attributes are removed or 400 Bad Request on failure.</returns>
		/// <response code="200">Attributes removed successfully.</response>
		/// <response code="400">Failed to remove attributes.</response>
		[HttpPut("remove-attribute")]
		public async Task<IActionResult> RemoveAttribute(int id, [FromBody] AttributeDeleteRequest request)
		{
			var result = await _productService.RemoveProductAttributeAsync(id, new AttributeDeleteInput
			{
				Attributes = request.Attributes
			});

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}

		#endregion

		#region Orders

		/// <summary>
		/// Retrieves all orders of the currently logged-in merchant.
		/// </summary>
		/// <param name="request">Pagination request.</param>
		/// <returns>Returns 200 OK with a list of orders or 400 Bad Request on failure.</returns>
		/// <response code="200">Orders retrieved successfully.</response>
		/// <response code="400">Failed to retrieve orders.</response>
		[HttpPost("get-all-orders")]
		public async Task<ActionResult> GetAllOrders([FromBody] PaginationRequest request)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var result = await _productService.ReadAllOrdersAsync(new PaginationInput
			{
				Page = request.PageNumber,
				PageSize = request.PageSize,
				FilterBy = userId
			});

			if (!result.IsSuccess)
				return BadRequest(result);

			return Ok(result.Data);
		}

		/// <summary>
		/// Retrieves a specific order by ID.
		/// </summary>
		/// <param name="id">Order ID.</param>
		/// <returns>Returns 200 OK with order details or 400 Bad Request on failure.</returns>
		/// <response code="200">Order retrieved successfully.</response>
		/// <response code="400">Failed to retrieve order.</response>
		[HttpGet("get-order-by-id/{id}")]
		public async Task<IActionResult> GetOrderById(int id)
		{
			var result = await _productService.ReadOrderByIdAsync(id);
			if (!result.IsSuccess)
				return BadRequest(result);

			return Ok(result.Data);
		}

		/// <summary>
		/// Updates the status of an order.
		/// </summary>
		/// <param name="id">Order ID.</param>
		/// <param name="request">New order status.</param>
		/// <returns>Returns 200 OK if status updated or 400 Bad Request on failure.</returns>
		/// <response code="200">Order status updated successfully.</response>
		/// <response code="400">Failed to update order status.</response>
		[HttpPut("update-order-status/{id}")]
		public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateRequest request)
		{
			var result = await _productService.UpdateOrderStatusAsync(id, new OrderStatusUpdateInput
			{
				Status = request.Status
			});

			if (!result.IsSuccess)
				return BadRequest(result);

			return Ok(result.Message);
		}

		#endregion
		[HttpGet("get-purchased-customers")]
		public async Task<IActionResult> GetPurchasedCustomers()
		{
			var shopId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var result = await _productService.ReadAllPurchasedCustomersAsync(shopId);

			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Data);
		}
	}
}
