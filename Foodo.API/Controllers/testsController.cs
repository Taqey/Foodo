using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Foodo.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class testsController : ControllerBase
	{
		[HttpGet("test-error")]
		public IActionResult TestError()
		{
			throw new ("Test server error!");
		}

	}
}
