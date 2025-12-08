using Foodo.API.Models.Request.Photo;
using Foodo.Application.Abstraction.Photo;
using Foodo.Application.Models.Input.Photo;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Foodo.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PhotosController : ControllerBase
	{
		private readonly IPhotoService _service;

		public PhotosController(IPhotoService service)
		{
			_service = service;
		}
		// GET: api/<PhotosController>
		//[HttpGet]
		//public IEnumerable<string> Get()
		//{
		//	return new string[] { "value1", "value2" };
		//}

		// GET api/<PhotosController>/5
		[HttpGet("get-user-photo")]
		public async Task<IActionResult> GetUserPhoto()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var result = await _service.ReadUserPhoto(new GetUserPhotoInput { Id = userId, });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}

			return Ok(result.Data);
		}

		// POST api/<PhotosController>
		[HttpPost("add-user-photo")]
		public async Task<IActionResult> AddUserPhoto([FromForm] AddPhotoRequest request)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
			var Enumvalue = (UserType)Enum.Parse(typeof(UserType), userRole);
			var result = await _service.AddUserPhoto(new AddPhotoInput { file = request.file, Id = userId, UserType = Enumvalue });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}

			return Ok(result.Message);
		}
		[HttpPost("add-merchant-photo")]
		public async Task<IActionResult> AddMerchantPhoto([FromForm] AddMerchantPhotoRequest request)
		{
			var result = await _service.AddUserPhoto(new AddPhotoInput { file = request.file, Id = request.MerchantId, UserType = UserType.Merchant });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}

			return Ok(result.Message);
		}
		//}		[HttpPost("add-user-photo")]
		//public async Task<IActionResult> AddUserPhoto([FromForm] AddPhotoRequest request)
		//{
		//	var userId=User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		//	var userRole=User.FindFirst(ClaimTypes.Role)?.Value;
		//	var Enumvalue=(UserType)Enum.Parse(typeof(UserType), userRole);
		//	var result = await _service.AddUserPhoto(new AddPhotoInput { file = request.file, Id = userId, UserType = Enumvalue });
		//	if (!result.IsSuccess)
		//	{
		//		return BadRequest(result.Message);
		//	}

		//	return Ok(result.Message);
		//}
		[HttpPost("add-product-photos")]

		public async Task<IActionResult> AddProductPhotos([FromForm] AddProductPhotosRequest request)
		{

			var result = await _service.AddProuctPhotos(new AddProductPhotosInput { Files = request.Files, ProductId = request.ProductId });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}

			return Ok(result.Data);
		}

		// PUT api/<PhotosController>/5
		[HttpPut("set-photo-main/{id}")]
		public async Task<IActionResult> Put(int id)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var result = await _service.SetProductPhotoMain(new SetPhotoMainInput { id = id });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}

			return Ok(result.Message);
		}

		// DELETE api/<PhotosController>/5
		//[HttpDelete("{id}")]
		//public void Delete(int id)
		//{
		//}
	}
}
