using Foodo.API.Models.Request;
using Foodo.API.Models.Request.Merchant;
using Foodo.Application.Abstraction.Merchant;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Merchant;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Foodo.API.Controllers
{
	/// <summary>
	/// Provides endpoints to manage products and orders for merchants, including
	/// creating, updating, deleting products, managing product attributes, 
	/// handling orders, and managing product categories.
	/// </summary>
	/// <remarks>
	/// <para>This controller handles all merchant-related operations:</para>
	/// <list type="bullet">
	///     <item><description>Retrieve, create, update, and delete products</description></item>
	///     <item><description>Add or remove product attributes</description></item>
	///     <item><description>Retrieve orders and update order status</description></item>
	///     <item><description>Manage product categories</description></item>
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
		private readonly IMerchantService _productService;
		private readonly ILogger<MerchantsController> _logger;

		public MerchantsController(IMerchantService productService, ILogger<MerchantsController> logger)
		{
			_productService = productService;
			_logger = logger;
		}

		#region Products

		/// <summary>
		/// Retrieves all products for the currently logged-in merchant.
		/// </summary>
		/// <param name="request">Pagination request.</param>
		/// <returns>Paginated list of products.</returns>
		/// <response code="200">Products retrieved successfully.</response>
		/// <response code="400">Failed to retrieve products.</response>
		[HttpPost("get-all-products")]
		public async Task<IActionResult> GetAllProducts([FromBody] PaginationRequest request)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			_logger.LogInformation(
				"Get all products attempt started | MerchantId={MerchantId} | Page={Page} | PageSize={PageSize} | TraceId={TraceId}",
				userId,
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
					"Get all products Validation failed | MerchantId={MerchantId} | Errors={Errors} | TraceId={TraceId}",
					userId,
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


			var result = await _productService.ReadAllProductsAsync(new ProductPaginationInput
			{
				Page = request.PageNumber,
				PageSize = request.PageSize,
				UserId = userId
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Failed to retrieve products | MerchantId={MerchantId} | Reason={Reason} | TraceId={TraceId}",
					userId,
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
				_logger.LogInformation(
					"Get all products returned empty | MerchantId={MerchantId} | TraceId={TraceId}",
					userId,
					HttpContext.TraceIdentifier
				);

				return Ok(new
				{
					message = "No products found",
					traceId = HttpContext.TraceIdentifier,
					data = result.Data

				});
			}

			_logger.LogInformation(
				"Get all products succeeded | MerchantId={MerchantId} | Count={Count} | TraceId={TraceId}",
				userId,
				result.Data.Items.Count,
				HttpContext.TraceIdentifier
			);

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
		/// <param name="id">ID of the product.</param>
		/// <returns>Product details.</returns>
		/// <response code="200">Product retrieved successfully.</response>
		/// <response code="400">Failed to retrieve product.</response>
		[HttpGet("get-product-by-id/{id}")]
		public async Task<IActionResult> GetProductById(int id)
		{
			_logger.LogInformation(
				"Get product by id attempt started | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate ID
			// ============================
			if (id <= 0)
			{
				_logger.LogWarning(
					"Invalid ProductId provided | ProductId={ProductId} | TraceId={TraceId}",
					id,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = "Invalid product ID",
					traceId = HttpContext.TraceIdentifier
				});
			}

			// ============================
			// Call Service
			// ============================
			var result = await _productService.ReadProductByIdAsync(id);

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Failed to retrieve product | ProductId={ProductId} | Reason={Reason} | TraceId={TraceId}",
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

			if (result.Data == null)
			{
				_logger.LogWarning(
					"Product not found | ProductId={ProductId} | TraceId={TraceId}",
					id,
					HttpContext.TraceIdentifier
				);

				return NotFound(new
				{
					message = "Product not found",
					traceId = HttpContext.TraceIdentifier
				});
			}

			_logger.LogInformation(
				"Product retrieved successfully | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				message = "Product retrieved successfully",
				traceId = HttpContext.TraceIdentifier,
				data = result.Data
			});
		}


		/// <summary>
		/// Creates a new product for the logged-in merchant.
		/// </summary>
		/// <param name="request">Product creation request.</param>
		/// <returns>Confirmation message.</returns>
		/// <response code="200">Product created successfully.</response>
		/// <response code="400">Failed to create product.</response>
		[HttpPost("create-product")]
		public async Task<IActionResult> CreateProduct([FromBody] ProductRequest request)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			_logger.LogInformation(
				"Create product attempt started | MerchantId={MerchantId} | ProductName={ProductName} | TraceId={TraceId}",
				userId,
				request?.ProductName,
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
					"Validation failed while creating product | MerchantId={MerchantId} | Errors={Errors} | TraceId={TraceId}",
					userId,
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
			var result = await _productService.CreateProductAsync(new ProductInput
			{
				Id = userId,
				ProductName = request.ProductName,
				ProductDescription = request.ProductDescription,
				Price = request.Price,
				Attributes = request.Attributes,
				Categories = request.Categories
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Product creation failed | MerchantId={MerchantId} | Reason={Reason} | TraceId={TraceId}",
					userId,
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
				"Product created successfully | MerchantId={MerchantId} | ProductName={ProductName} | TraceId={TraceId}",
				userId,
				request.ProductName,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				message = "Product created successfully",
				traceId = HttpContext.TraceIdentifier
			});
		}


		/// <summary>
		/// Updates an existing product by its ID.
		/// </summary>
		/// <param name="id">ID of the product.</param>
		/// <param name="request">Updated product details.</param>
		/// <returns>Confirmation message.</returns>
		/// <response code="200">Product updated successfully.</response>
		/// <response code="400">Failed to update product.</response>
		[HttpPut("update-product/{id}")]
		public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateRequest request)
		{
			_logger.LogInformation(
				"Update product attempt started | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate Product ID
			// ============================
			if (id <= 0)
			{
				_logger.LogWarning(
					"Invalid ProductId provided | ProductId={ProductId} | TraceId={TraceId}",
					id,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = "Invalid product ID",
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

				_logger.LogWarning(
					"Validation failed while updating product | ProductId={ProductId} | Errors={Errors} | TraceId={TraceId}",
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
			var result = await _productService.UpdateProductAsync(new ProductUpdateInput
			{
				productId = id,
				ProductName = request.ProductName,
				ProductDescription = request.ProductDescription,
				Price = request.Price
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Product update failed | ProductId={ProductId} | Reason={Reason} | TraceId={TraceId}",
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
				"Product updated successfully | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				message = "Product updated successfully",
				traceId = HttpContext.TraceIdentifier
			});
		}

		/// <summary>
		/// Deletes a product by its ID.
		/// </summary>
		/// <param name="id">ID of the product.</param>
		/// <returns>Confirmation message.</returns>
		/// <response code="200">Product deleted successfully.</response>
		/// <response code="400">Failed to delete product.</response>
		[HttpDelete("delete-product/{id}")]
		public async Task<IActionResult> DeleteProduct(int id)
		{
			_logger.LogInformation(
				"Delete product attempt started | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate Product ID
			// ============================
			if (id <= 0)
			{
				_logger.LogWarning(
					"Invalid ProductId provided | ProductId={ProductId} | TraceId={TraceId}",
					id,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = "Invalid product ID",
					traceId = HttpContext.TraceIdentifier
				});
			}

			// ============================
			// Call Service
			// ============================
			var result = await _productService.DeleteProductAsync(id);

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Product deletion failed | ProductId={ProductId} | Reason={Reason} | TraceId={TraceId}",
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
				"Product deleted successfully | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				message = "Product deleted successfully",
				traceId = HttpContext.TraceIdentifier
			});
		}


		#endregion

		#region Product Attributes

		/// <summary>
		/// Adds attributes to an existing product.
		/// </summary>
		/// <param name="id">ID of the product.</param>
		/// <param name="request">Attributes to add.</param>
		/// <returns>Confirmation message.</returns>
		/// <response code="200">Attributes added successfully.</response>
		/// <response code="400">Failed to add attributes.</response>
		[HttpPut("add-attribute")]
		public async Task<IActionResult> AddAttribute(int id, [FromBody] AttributeCreateRequest request)
		{
			_logger.LogInformation(
				"Add product attributes attempt started | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate Product ID
			// ============================
			if (id <= 0)
			{
				_logger.LogWarning(
					"Invalid ProductId provided | ProductId={ProductId} | TraceId={TraceId}",
					id,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = "Invalid product ID",
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

				_logger.LogWarning(
					"Validation failed while adding attributes | ProductId={ProductId} | Errors={Errors} | TraceId={TraceId}",
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
			var result = await _productService.AddProductAttributeAsync(id, new AttributeCreateInput
			{
				Attributes = request.Attributes
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Adding attributes failed | ProductId={ProductId} | Reason={Reason} | TraceId={TraceId}",
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
				"Attributes added successfully | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				message = "Attributes added successfully",
				traceId = HttpContext.TraceIdentifier
			});
		}


		/// <summary>
		/// Removes attributes from an existing product.
		/// </summary>
		/// <param name="id">ID of the product.</param>
		/// <param name="request">Attributes to remove.</param>
		/// <returns>Confirmation message.</returns>
		/// <response code="200">Attributes removed successfully.</response>
		/// <response code="400">Failed to remove attributes.</response>
		[HttpPut("remove-attribute")]
		public async Task<IActionResult> RemoveAttribute(int id, [FromBody] AttributeDeleteRequest request)
		{
			_logger.LogInformation(
				"Remove product attributes attempt started | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate Product ID
			// ============================
			if (id <= 0)
			{
				_logger.LogWarning(
					"Invalid ProductId provided | ProductId={ProductId} | TraceId={TraceId}",
					id,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = "Invalid product ID",
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

				_logger.LogWarning(
					"Validation failed while removing attributes | ProductId={ProductId} | Errors={Errors} | TraceId={TraceId}",
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
			var result = await _productService.RemoveProductAttributeAsync(id, new AttributeDeleteInput
			{
				Attributes = request.Attributes
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Removing attributes failed | ProductId={ProductId} | Reason={Reason} | TraceId={TraceId}",
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
				"Attributes removed successfully | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				message = "Attributes removed successfully",
				traceId = HttpContext.TraceIdentifier
			});
		}


		#endregion

		#region Orders

		/// <summary>
		/// Retrieves all orders of the currently logged-in merchant.
		/// </summary>
		/// <param name="request">Pagination request.</param>
		/// <returns>List of orders.</returns>
		/// <response code="200">Orders retrieved successfully.</response>
		/// <response code="400">Failed to retrieve orders.</response>
		[HttpPost("get-all-orders")]
		public async Task<IActionResult> GetAllOrders([FromBody] PaginationRequest request)
		{
			_logger.LogInformation(
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

			// ============================
			// Call Service
			// ============================
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var result = await _productService.ReadAllOrdersAsync(new ProductPaginationInput
			{
				Page = request.PageNumber,
				PageSize = request.PageSize,
				UserId = userId
			});

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

			_logger.LogInformation(
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
		[HttpGet("get-order-by-id/{id}")]
		public async Task<IActionResult> GetOrderById(int id)
		{
			_logger.LogInformation(
				"Get order by ID attempt started | OrderId={OrderId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate Order ID
			// ============================
			if (id <= 0)
			{
				_logger.LogWarning(
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
			var result = await _productService.ReadOrderByIdAsync(id);

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
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

			_logger.LogInformation(
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
		[HttpPut("update-order-status/{id}")]
		public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateRequest request)
		{
			_logger.LogInformation(
				"Update order status attempt started | OrderId={OrderId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate Order ID
			// ============================
			if (id <= 0)
			{
				_logger.LogWarning(
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

				_logger.LogWarning(
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
			var result = await _productService.UpdateOrderStatusAsync(id, new OrderStatusUpdateInput
			{
				Status = request.Status
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
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

			_logger.LogInformation(
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


		#endregion

		#region Product Categories

		/// <summary>
		/// Adds categories to a product.
		/// </summary>
		/// <param name="productId">ID of the product.</param>
		/// <param name="request">Categories to add.</param>
		/// <returns>Confirmation message.</returns>
		/// <response code="200">Categories added successfully.</response>
		/// <response code="400">Failed to add categories.</response>
		[HttpPut("add-categories/{productId}")]
		public async Task<IActionResult> AddCategories(int productId, [FromBody] CategoryRequest request)
		{
			_logger.LogInformation(
				"Add categories attempt started | ProductId={ProductId} | TraceId={TraceId}",
				productId,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate Product ID
			// ============================
			if (productId <= 0)
			{
				_logger.LogWarning(
					"Invalid ProductId provided | ProductId={ProductId} | TraceId={TraceId}",
					productId,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = "Invalid product ID",
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

				_logger.LogWarning(
					"Validation failed while adding categories | ProductId={ProductId} | Errors={Errors} | TraceId={TraceId}",
					productId,
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
			var result = await _productService.AddProductCategoriesAsync(new ProductCategoryInput
			{
				restaurantCategories = request.Categories,
				ProductId = productId
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Add categories failed | ProductId={ProductId} | Reason={Reason} | TraceId={TraceId}",
					productId,
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
				"Categories added successfully | ProductId={ProductId} | TraceId={TraceId}",
				productId,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				message = "Categories added successfully",
				traceId = HttpContext.TraceIdentifier
			});
		}


		/// <summary>
		/// Removes categories from a product.
		/// </summary>
		/// <param name="productId">ID of the product.</param>
		/// <param name="request">Categories to remove.</param>
		/// <returns>Confirmation message.</returns>
		/// <response code="200">Categories removed successfully.</response>
		/// <response code="400">Failed to remove categories.</response>
		[HttpPut("remove-categories/{productId}")]
		public async Task<IActionResult> RemoveCategories(int productId, [FromBody] CategoryRequest request)
		{
			_logger.LogInformation(
				"Remove categories attempt started | ProductId={ProductId} | TraceId={TraceId}",
				productId,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate Product ID
			// ============================
			if (productId <= 0)
			{
				_logger.LogWarning(
					"Invalid ProductId provided | ProductId={ProductId} | TraceId={TraceId}",
					productId,
					HttpContext.TraceIdentifier
				);

				return BadRequest(new
				{
					message = "Invalid product ID",
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

				_logger.LogWarning(
					"Validation failed while removing categories | ProductId={ProductId} | Errors={Errors} | TraceId={TraceId}",
					productId,
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
			var result = await _productService.RemoveProductCategoriesAsync(new ProductCategoryInput
			{
				ProductId = productId,
				restaurantCategories = request.Categories
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
					"Remove categories failed | ProductId={ProductId} | Reason={Reason} | TraceId={TraceId}",
					productId,
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
				"Categories removed successfully | ProductId={ProductId} | TraceId={TraceId}",
				productId,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				message = "Categories removed successfully",
				traceId = HttpContext.TraceIdentifier
			});
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
		public async Task<IActionResult> GetPurchasedCustomers([FromBody] PaginationRequest request)
		{
			var shopId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			_logger.LogInformation(
				"Get purchased customers attempt started | ShopId={ShopId} | Page={Page} | PageSize={PageSize} | TraceId={TraceId}",
				shopId,
				request.PageNumber,
				request.PageSize,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate ModelState
			// ============================
			if (!ModelState.IsValid)
			{
				var errors = string.Join(" | ",
					ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage));

				_logger.LogWarning(
					"Validation failed while retrieving purchased customers | ShopId={ShopId} | Errors={Errors} | TraceId={TraceId}",
					shopId,
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
			var result = await _productService.ReadAllPurchasedCustomersAsync(new ProductPaginationInput
			{
				Page = request.PageNumber,
				PageSize = request.PageSize,
				UserId = shopId
			});

			if (!result.IsSuccess)
			{
				_logger.LogWarning(
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

			_logger.LogInformation(
				"Purchased customers retrieved successfully | ShopId={ShopId} | Count={Count} | TraceId={TraceId}",
				shopId,
				result.Data?.Items?.Count ?? 0,
				HttpContext.TraceIdentifier
			);
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
