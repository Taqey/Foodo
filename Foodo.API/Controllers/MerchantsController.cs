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
	/// Provides endpoints to manage products for merchants, including
	/// creating, updating, deleting, reading products, and managing attributes.
	/// </summary>
	/// <remarks>
	/// <para>This controller handles all product-related operations for merchant users:</para>
	/// <list type="bullet">
	///     <item>
	///         <description>Retrieve all products for the logged-in merchant</description>
	///     </item>
	///     <item>
	///         <description>Retrieve a product by its ID</description>
	///     </item>
	///     <item>
	///         <description>Create, update, or delete a product</description>
	///     </item>
	///     <item>
	///         <description>Add or remove product attributes</description>
	///     </item>
	/// </list>
	/// <para>
	/// All input models are received using <c>[FromBody]</c>, and endpoints require Bearer authentication.
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

		/// <summary>
		/// Retrieves all products of the currently logged-in merchant.
		/// </summary>
		/// <returns>
		/// Returns a list of products if successful.
		/// </returns>
		/// <response code="200">Successfully retrieved all products.</response>
		/// <response code="400">Failed to retrieve products.</response>
		[HttpGet("get-all-products")]
		public async Task<ActionResult> Get()
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
		/// <param name="id">ID of the product to retrieve.</param>
		/// <returns>The product details if found.</returns>
		/// <response code="200">Product retrieved successfully.</response>
		/// <response code="400">Failed to retrieve the product.</response>
		[HttpGet("get-product-by-id/{id}")]
		public async Task<IActionResult> Get(int id)
		{
			var result = await _productService.ReadProductByIdAsync(id);
			if (!result.IsSuccess)
				return BadRequest(result);

			return Ok(result.Data);
		}

		/// <summary>
		/// Creates a new product for the logged-in merchant.
		/// </summary>
		/// <param name="request">Product creation request including name, description, price, and attributes.</param>
		/// <returns>A success message if the product is created.</returns>
		/// <response code="200">Product created successfully.</response>
		/// <response code="400">Failed to create the product.</response>
		[HttpPost("create-product")]
		public async Task<IActionResult> Post([FromBody] ProductRequest request)
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
		/// <param name="request">Product update data including name, description, price, and attributes.</param>
		/// <returns>A success message if the product is updated.</returns>
		/// <response code="200">Product updated successfully.</response>
		/// <response code="400">Failed to update the product.</response>
		[HttpPut("update-product/{id}")]
		public async Task<IActionResult> Put(int id, [FromBody] ProductRequest request)
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
		/// <returns>A success message if the product is deleted.</returns>
		/// <response code="200">Product deleted successfully.</response>
		/// <response code="400">Failed to delete the product.</response>
		[HttpDelete("delete-product/{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _productService.DeleteProductAsync(id);
			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}

		/// <summary>
		/// Adds attributes to an existing product.
		/// </summary>
		/// <param name="id">ID of the product.</param>
		/// <param name="request">Attributes to add.</param>
		/// <returns>A success message if attributes are added.</returns>
		/// <response code="200">Attributes added successfully.</response>
		/// <response code="400">Failed to add attributes.</response>
		[HttpPut("add-attribute")]
		public async Task<IActionResult> AddAttribute(int id, [FromBody] AttributeCreateRequest request)
		{
			var result = await _productService.AddProductAttributeAsync(id, new AttributeCreateInput { Attributes = request.Attributes });
			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}

		/// <summary>
		/// Removes attributes from an existing product.
		/// </summary>
		/// <param name="id">ID of the product.</param>
		/// <param name="request">Attributes to remove.</param>
		/// <returns>A success message if attributes are removed.</returns>
		/// <response code="200">Attributes removed successfully.</response>
		/// <response code="400">Failed to remove attributes.</response>
		[HttpPut("remove-attribute")]
		public async Task<IActionResult> RemoveAttribute(int id, [FromBody] AttributeDeleteRequest request)
		{
			var result = await _productService.RemoveProductAttributeAsync(id, new AttributeDeleteInput { Attributes = request.Attributes });
			if (!result.IsSuccess)
				return BadRequest(result.Message);

			return Ok(result.Message);
		}
	}
}
