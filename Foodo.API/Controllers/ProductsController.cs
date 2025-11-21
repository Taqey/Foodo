using Foodo.API.Models.Request;
using Foodo.Application.Abstraction.Merchant;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Foodo.API.Controllers
{
	[Authorize(Roles =nameof(UserType.Merchant))]
	[Route("api/[controller]")]
	[ApiController]
	public class ProductsController : ControllerBase
	{
		private readonly IProductService _productService;

		public ProductsController(IProductService productService)
		{
			_productService = productService;
		}
		// GET: api/<ProductsController>
		[HttpGet]
		public async Task<ActionResult> Get()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var result = await _productService.ReadAllProductsAsync(userId);

			if (!result.IsSuccess)
			{
				return BadRequest(result);
			}

			return Ok(result.Data);
		}



		// GET api/<ProductsController>/5
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(int id)
		{
			var result = await _productService.ReadProductByIdAsync(id);
			if (!result.IsSuccess)
			{
				return BadRequest(result);
			}
			return Ok(result.Data);
		}

		// POST api/<ProductsController>
		[HttpPost]
		public async Task<IActionResult> Post([FromBody] ProductRequest request)
		{
			var Id=User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var result = await _productService.CreateProductAsync(new ProductInput
			{
				Id=Id,
				ProductName = request.ProductName,
				ProductDescription = request.ProductDescription,
				Price = request.Price,
				Attributes = request.Attributes
			});
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}
			return Ok(result.Message);

		}

		// PUT api/<ProductsController>/5
		[HttpPut("{id}")]
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
			{
				return BadRequest(result.Message);
			}
			return Ok(result.Message);


		}

		// DELETE api/<ProductsController>/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _productService.DeleteProductAsync(id);
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}
			return Ok(result.Message);

		}
		[HttpPut("add-attribute")]
		public async Task<IActionResult> AddAttribute(int id,[FromBody] AttributeCreateRequest request)
		{
			var result = await _productService.AddProductAttributeAsync(id, new AttributeCreateInput { Attributes = request.Attributes });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}
			return Ok(result.Message);
		}
		[HttpPut("remove-attribute")]
		public async Task<IActionResult> RemoveAttribute(int id,[FromBody] AttributeDeleteRequest request)
		{
			var result = await _productService.RemoveProductAttributeAsync(id, new AttributeDeleteInput { Attributes = request.Attributes });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}
			return Ok(result.Message);
		}
	}
}
