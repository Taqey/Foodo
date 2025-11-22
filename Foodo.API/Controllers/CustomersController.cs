using Foodo.API.Models.Request;
using Foodo.Application.Abstraction.Customer;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Input;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Foodo.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CustomersController : ControllerBase
	{
		private readonly ICustomerService _service;

		public CustomersController(ICustomerService service)
		{
			_service = service;
		}
		// GET: api/<CustomersController>
		[HttpPost("get-all-products")]
		public async Task<IActionResult> GetAllproducts( PaginationRequest request)
		{
			var result=await _service.ReadAllProducts(new PaginationInput { Page=request.PageNumber, PageSize=request.PageSize });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}
			return Ok(result.Data);
		}
		[HttpPost("get-all-shops")]
		public async Task<IActionResult> GetAllShops(PaginationRequest request)
		{
			var result = await _service.ReadAllShops(new PaginationInput { Page = request.PageNumber, PageSize = request.PageSize });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}
			return Ok(result.Data);
		}
		// GET api/<CustomersController>/5
		[HttpGet("get-product-by-id/{id}")]
		public async Task<IActionResult> GetProductbyId(int id)
		{
			var result = await _service.ReadProductById(new ItemByIdInput { ItemId = id.ToString() });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}
			return Ok(result.Data);
		}
		// GET api/<CustomersController>/5
		[HttpGet("get-shop-by-id/{id}")]
		public async Task<IActionResult> GetShopById(string id)
		{
			var result = await _service.ReadShopById(new ItemByIdInput { ItemId = id.ToString() });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}
			return Ok(result.Data);
		}
		[HttpGet("get-all-products-by-category")]
		public async Task<IActionResult> GetAllproductsByCategory()
		{
			return Ok();
		}
		[HttpGet("get-all-Shops-by-category")]
		public async Task<IActionResult> GetAllShopsByCategory()
		{
			return Ok();
		}





		// POST api/<CustomersController>
		[Authorize(Roles = nameof(UserType.Customer))]

		[HttpPost("place-order")]
		public async Task<IActionResult> Post([FromBody] CreateOrderRequest request)
		{
			var id=User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var result = await _service.PlaceOrder(new CreateOrderInput { CustomerId=id, Items=request.Items });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}
			return Ok(result);
		}

		// PUT api/<CustomersController>/5
		[Authorize(Roles = nameof(UserType.Customer))]

		[HttpPut("edit-order/{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/<CustomersController>/5
		[Authorize(Roles = nameof(UserType.Customer))]

		[HttpDelete("cancel-order/{id}")]
		public void Delete(int id)
		{
		}
	}
}
