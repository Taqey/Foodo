using Foodo.API.Filters;
using Foodo.API.Models.Request;
using Foodo.API.Models.Request.Merchant;
using Foodo.Application.Commands.Products.AddProductAttribute;
using Foodo.Application.Commands.Products.AddProductCategory;
using Foodo.Application.Commands.Products.CreateProduct;
using Foodo.Application.Commands.Products.DeleteProduct;
using Foodo.Application.Commands.Products.RemoveProductAttribute;
using Foodo.Application.Commands.Products.RemoveProductCategory;
using Foodo.Application.Commands.Products.UpdateProduct;
using Foodo.Application.Factory.Product;
using Foodo.Application.Models.Enums;
using Foodo.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
		//private readonly ICustomerProductService _customerService;
		//private readonly IMerchantProductService _merchantService;
		private readonly IProductStrategyFactory _factory;
		private readonly IMediator _mediator;

		public ProductsController(IProductStrategyFactory factory, IMediator mediator)
		{
			//_customerService = customerService;
			//_merchantService = merchantService;
			_factory = factory;
			_mediator = mediator;
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
			[FromQuery] FoodCategory? categoryId = null,
			[FromQuery] string? restaurantId = null,
			[FromQuery] ProductOrderBy? orderBy = null,
			[FromQuery] OrderingDirection? orderingDirection = null
			)
		{
			var strategy = _factory.GetProductsStrategy(User, request.PageNumber, request.PageSize, categoryId, restaurantId, orderBy, orderingDirection);
			var result = await _mediator.Send(strategy);
			if (!result.IsSuccess)
			{
				Log.Warning("Get products failed | Reason={Reason} | TraceId={TraceId}", result.Message, HttpContext.TraceIdentifier);
				return BadRequest(new { message = result.Message, traceId = HttpContext.TraceIdentifier });
			}

			if (result.Data == null)
			{
				Log.Warning("Get products returned empty | TraceId={TraceId}", HttpContext.TraceIdentifier);
				return Ok(new { message = "No products found", traceId = HttpContext.TraceIdentifier, data = result.Data });
			}
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
		[ServiceFilter(typeof(ValidateIdFilter))]
		public async Task<IActionResult> GetProductbyId(int id)
		{
			var strategy = _factory.GetProductStrategy(User, id);
			var result = await _mediator.Send(strategy);

			if (!result.IsSuccess)
			{
				Log.Warning("Get all Products By Id  failed: Product not found | ProductId={ProductId} | TraceId={TraceId}", id, HttpContext.TraceIdentifier);

				return NotFound(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
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
			var result = await _mediator.Send(new CreateProductCommand
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
				Log.Warning("Product creation failed | MerchantId={MerchantId} | Reason={Reason} | TraceId={TraceId}", userId, result.Message, HttpContext.TraceIdentifier);

				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
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
		[ServiceFilter(typeof(ValidateIdFilter))]
		public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateRequest request)
		{

			var result = await _mediator.Send(new UpdateProductCommand
			{
				productId = id,
				ProductName = request.ProductName,
				ProductDescription = request.ProductDescription,
				Price = request.Price
			});

			if (!result.IsSuccess)
			{
				Log.Warning("Product update failed | ProductId={ProductId} | Reason={Reason} | TraceId={TraceId}", id, result.Message, HttpContext.TraceIdentifier);
				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
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
		[ServiceFilter(typeof(ValidateIdFilter))]
		public async Task<IActionResult> DeleteProduct(int id)
		{
			var result = await _mediator.Send(new DeleteProductCommand { productId = id, UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value });

			if (!result.IsSuccess)
			{
				Log.Warning("Product deletion failed | ProductId={ProductId} | Reason={Reason} | TraceId={TraceId}", id, result.Message, HttpContext.TraceIdentifier);
				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
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
		[ServiceFilter(typeof(ValidateIdFilter))]
		public async Task<IActionResult> AddAttribute(int id, [FromBody] AttributeCreateRequest request)
		{
			var result = await _mediator.Send(new AddProductAttributeCommand
			{
				Attributes = request.Attributes,
				ProductId = id

			});

			if (!result.IsSuccess)
			{
				Log.Warning("Adding attributes failed | ProductId={ProductId} | Reason={Reason} | TraceId={TraceId}", id, result.Message, HttpContext.TraceIdentifier);
				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
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
		[ServiceFilter(typeof(ValidateIdFilter))]
		public async Task<IActionResult> RemoveAttribute(int id, [FromBody] AttributeDeleteRequest request)
		{
			var result = await _mediator.Send(new RemoveProductAttributeCommand
			{
				Attributes = request.Attributes,
				ProductId = id
			});

			if (!result.IsSuccess)
			{
				Log.Warning("Removing attributes failed | ProductId={ProductId} | Reason={Reason} | TraceId={TraceId}", id, result.Message, HttpContext.TraceIdentifier);

				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
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
		[ServiceFilter(typeof(ValidateIdFilter))]
		public async Task<IActionResult> AddCategories(int Id, [FromBody] CategoryRequest request)
		{
			var result = await _mediator.Send(new AddProductCategoryCommand
			{
				restaurantCategories = request.Categories,
				ProductId = Id
			});

			if (!result.IsSuccess)
			{
				Log.Warning("Add categories failed | ProductId={ProductId} | Reason={Reason} | TraceId={TraceId}", Id, result.Message, HttpContext.TraceIdentifier);

				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
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
		[ServiceFilter(typeof(ValidateIdFilter))]
		public async Task<IActionResult> RemoveCategories(int Id, [FromBody] CategoryRequest request)
		{
			var result = await _mediator.Send(new RemoveProductCategoryCommand
			{
				ProductId = Id,
				restaurantCategories = request.Categories
			});

			if (!result.IsSuccess)
			{
				Log.Warning("Remove categories failed | ProductId={ProductId} | Reason={Reason} | TraceId={TraceId}", Id, result.Message, HttpContext.TraceIdentifier);
				return BadRequest(new
				{
					message = result.Message,
					traceId = HttpContext.TraceIdentifier
				});
			}
			return Ok(new
			{
				message = "Categories removed successfully",
				traceId = HttpContext.TraceIdentifier
			});
		}
		#endregion

	}
}
