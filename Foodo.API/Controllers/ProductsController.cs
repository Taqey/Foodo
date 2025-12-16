using Foodo.API.Models.Request;
using Foodo.API.Models.Request.Customer;
using Foodo.API.Models.Request.Merchant;
using Foodo.Application.Abstraction.Customer;
using Foodo.Application.Abstraction.Merchant;
using Foodo.Application.Factory.Product;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Product;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Input.Merchant;
using Foodo.Application.Models.Response;
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
	public class ProductsController : ControllerBase
	{
		private readonly ICustomerService _customerService;
		private readonly IMerchantService _merchantService;
		private readonly IProductStrategyFactory _factory;

		public ProductsController(ICustomerService customerService ,IMerchantService merchantService,IProductStrategyFactory factory)
		{
			_customerService = customerService;
			_merchantService = merchantService;
			_factory = factory;
		}
		#region Product

		/// <summary>
		/// Retrieves products with optional filters for category and restaurant, with pagination.
		/// </summary>
		/// <param name="request">Pagination parameters.</param>
		/// <param name="categoryId">Optional category filter.</param>
		/// <param name="restaurantId">Optional restaurant filter.</param>
		/// <returns>Filtered and paginated list of products.</returns>
		[HttpGet]
		[EnableRateLimiting("TokenBucketPolicy")]
		public async Task<IActionResult> GetProducts(
			[FromQuery] PaginationRequest request,
			[FromQuery] int? categoryId = null,
			[FromQuery] string? restaurantId = null)
		{
			Log.Information(
				"Get products attempt started | Category={Category} | Restaurant={Restaurant} | Page={Page} | PageSize={PageSize} | TraceId={TraceId}",
				categoryId, restaurantId, request.PageNumber, request.PageSize, HttpContext.TraceIdentifier
			);

			if (!ModelState.IsValid)
			{
				var errors = string.Join(" | ",
					ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

				Log.Warning("Validation failed | Errors={Errors} | TraceId={TraceId}", errors, HttpContext.TraceIdentifier);
				return BadRequest(new { message = errors, traceId = HttpContext.TraceIdentifier });
			}
			var strategy = _factory.GetStrategy(User);
			ApiResponse<PaginationDto<ProductBaseDto>> result = null;
			ApiResponse<PaginationDto<CustomerProductDto>> resultDto = null;
			if (categoryId.HasValue)
			{
				resultDto = await _customerService.ReadProductsByCategory(new ProductPaginationByCategoryInput { Page = request.PageNumber, PageSize = request.PageSize, Category = (FoodCategory)categoryId });
			}
			else if (!string.IsNullOrEmpty(restaurantId))
			{
				resultDto = await _customerService.ReadProductsByShop(new ProductPaginationByShopInput { MerchantId = restaurantId, Page = request.PageNumber, PageSize = request.PageSize });

			}
			else
			{
				result = await strategy.ReadProducts(new ProductPaginationInput { Page = request.PageNumber, PageSize = request.PageSize });
			}

			
			if (!result.IsSuccess)
			{
				Log.Warning("Get products failed | Reason={Reason} | TraceId={TraceId}", result.Message, HttpContext.TraceIdentifier);
				return BadRequest(new { message = result.Message, traceId = HttpContext.TraceIdentifier });
			}

			if (result.Data == null || result.Data.Items.Count == 0)
			{
				Log.Warning("Get products returned empty | TraceId={TraceId}", HttpContext.TraceIdentifier);
				return Ok(new { message = "No products found", traceId = HttpContext.TraceIdentifier, data = result.Data });
			}

			Log.Information(
				"Get products succeeded | Count={Count} | TraceId={TraceId}",
				result.Data.Items.Count, HttpContext.TraceIdentifier
			);

			return Ok(new { message = "Products retrieved successfully", traceId = HttpContext.TraceIdentifier, data = result.Data });
		}


		/// <summary>
		/// Retrieves a product by its ID.
		/// </summary>
		/// <param name="id">Product ID.</param>
		/// <returns>Product data if found.</returns>
		/// <response code="200">Product retrieved successfully.</response>
		/// <response code="400">Failed to retrieve product.</response>
		/// <response code="404">Product not found.</response>
		[HttpGet("{id}")]
		[EnableRateLimiting("TokenBucketPolicy")]
		public async Task<IActionResult> GetProductbyId(int id)
		{
			Log.Information(
				"GetProductById attempt started | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			if (id <= 0)
			{
				Log.Warning(
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
			var strategy = _factory.GetStrategy(User);

			var result = await strategy.ReadProduct(new ItemByIdInput
			{
				ItemId = id.ToString()
			});

			if (!result.IsSuccess)
			{
				Log.Warning(
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

			Log.Information(
				"Get all Products By Id  succeeded | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			return Ok(result.Data);
		}

		/// <summary>
		/// Creates a new product for the logged-in merchant.
		/// </summary>
		/// <param name="request">Product creation request.</param>
		/// <returns>Confirmation message.</returns>
		/// <response code="200">Product created successfully.</response>
		/// <response code="400">Failed to create product.</response>
		[HttpPost]
		[EnableRateLimiting("FixedWindowPolicy")]
		[Authorize(Roles = nameof(UserType.Merchant))]

		public async Task<IActionResult> CreateProduct([FromBody] ProductRequest request)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			Log.Information(
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

				Log.Warning(
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
			var result = await _merchantService.CreateProductAsync(new ProductInput
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
				Log.Warning(
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

			Log.Information(
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
		[HttpPut("{id}")]
		[EnableRateLimiting("SlidingWindowPolicy")]
		[Authorize(Roles = nameof(UserType.Merchant))]

		public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateRequest request)
		{
			Log.Information(
				"Update product attempt started | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate Product ID
			// ============================
			if (id <= 0)
			{
				Log.Warning(
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

				Log.Warning(
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
			var result = await _merchantService.UpdateProductAsync(new ProductUpdateInput
			{
				productId = id,
				ProductName = request.ProductName,
				ProductDescription = request.ProductDescription,
				Price = request.Price
			});

			if (!result.IsSuccess)
			{
				Log.Warning(
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

			Log.Information(
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
		[HttpDelete("{id}")]
		[EnableRateLimiting("FixedWindowPolicy")]
		[Authorize(Roles = nameof(UserType.Merchant))]
		public async Task<IActionResult> DeleteProduct(int id)
		{
			Log.Information(
				"Delete product attempt started | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate Product ID
			// ============================
			if (id <= 0)
			{
				Log.Warning(
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
			var result = await _merchantService.DeleteProductAsync(id);

			if (!result.IsSuccess)
			{
				Log.Warning(
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

			Log.Information(
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
		[HttpPut("{id}/attributes")]
		[EnableRateLimiting("SlidingWindowPolicy")]
		[Authorize(Roles = nameof(UserType.Merchant))]
		public async Task<IActionResult> AddAttribute(int id, [FromBody] AttributeCreateRequest request)
		{
			Log.Information(
				"Add product attributes attempt started | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate Product ID
			// ============================
			if (id <= 0)
			{
				Log.Warning(
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

				Log.Warning(
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
			var result = await _merchantService.AddProductAttributeAsync(id, new AttributeCreateInput
			{
				Attributes = request.Attributes
			});

			if (!result.IsSuccess)
			{
				Log.Warning(
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

			Log.Information(
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
		[HttpDelete("{id}/attributes")]
		[EnableRateLimiting("SlidingWindowPolicy")]
		[Authorize(Roles = nameof(UserType.Merchant))]
		public async Task<IActionResult> RemoveAttribute(int id, [FromBody] AttributeDeleteRequest request)
		{
			Log.Information(
				"Remove product attributes attempt started | ProductId={ProductId} | TraceId={TraceId}",
				id,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate Product ID
			// ============================
			if (id <= 0)
			{
				Log.Warning(
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

				Log.Warning(
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
			var result = await _merchantService.RemoveProductAttributeAsync(id, new AttributeDeleteInput
			{
				Attributes = request.Attributes
			});

			if (!result.IsSuccess)
			{
				Log.Warning(
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

			Log.Information(
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

		#region Product Categories

		/// <summary>
		/// Adds categories to a product.
		/// </summary>
		/// <param name="Id">ID of the product.</param>
		/// <param name="request">Categories to add.</param>
		/// <returns>Confirmation message.</returns>
		/// <response code="200">Categories added successfully.</response>
		/// <response code="400">Failed to add categories.</response>
		[HttpPut("{Id}/categories")]
		[EnableRateLimiting("SlidingWindowPolicy")]
		[Authorize(Roles = nameof(UserType.Merchant))]
		public async Task<IActionResult> AddCategories(int Id, [FromBody] CategoryRequest request)
		{
			Log.Information(
				"Add categories attempt started | ProductId={ProductId} | TraceId={TraceId}",
				Id,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate Product ID
			// ============================
			if (Id <= 0)
			{
				Log.Warning(
					"Invalid ProductId provided | ProductId={ProductId} | TraceId={TraceId}",
					Id,
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

				Log.Warning(
					"Validation failed while adding categories | ProductId={ProductId} | Errors={Errors} | TraceId={TraceId}",
					Id,
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
			var result = await _merchantService.AddProductCategoriesAsync(new ProductCategoryInput
			{
				restaurantCategories = request.Categories,
				ProductId = Id
			});

			if (!result.IsSuccess)
			{
				Log.Warning(
					"Add categories failed | ProductId={ProductId} | Reason={Reason} | TraceId={TraceId}",
					Id,
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
				"Categories added successfully | ProductId={ProductId} | TraceId={TraceId}",
				Id,
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
		[HttpDelete("{Id}/categories")]
		[EnableRateLimiting("SlidingWindowPolicy")]
		[Authorize(Roles = nameof(UserType.Merchant))]
		public async Task<IActionResult> RemoveCategories(int Id, [FromBody] CategoryRequest request)
		{
			Log.Information(
				"Remove categories attempt started | ProductId={ProductId} | TraceId={TraceId}",
				Id,
				HttpContext.TraceIdentifier
			);

			// ============================
			// Validate Product ID
			// ============================
			if (Id <= 0)
			{
				Log.Warning(
					"Invalid ProductId provided | ProductId={ProductId} | TraceId={TraceId}",
					Id,
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

				Log.Warning(
					"Validation failed while removing categories | ProductId={ProductId} | Errors={Errors} | TraceId={TraceId}",
					Id,
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
			var result = await _merchantService.RemoveProductCategoriesAsync(new ProductCategoryInput
			{
				ProductId = Id,
				restaurantCategories = request.Categories
			});

			if (!result.IsSuccess)
			{
				Log.Warning(
					"Remove categories failed | ProductId={ProductId} | Reason={Reason} | TraceId={TraceId}",
					Id,
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
				"Categories removed successfully | ProductId={ProductId} | TraceId={TraceId}",
				Id,
				HttpContext.TraceIdentifier
			);

			return Ok(new
			{
				message = "Categories removed successfully",
				traceId = HttpContext.TraceIdentifier
			});
		}


		#endregion

	}
}
