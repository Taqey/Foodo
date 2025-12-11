using Foodo.Application.Abstraction.InfraRelated;
using Microsoft.AspNetCore.Mvc;

namespace Foodo.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TestCacheController : ControllerBase
	{
		private readonly ICacheService _cacheService;

		public TestCacheController(ICacheService cacheService)
		{
			_cacheService = cacheService;
		}

		[HttpGet("test-fusion")]
		public IActionResult TestFusionCache()
		{
			string cacheKey = "test:fusioncache";

			// حاول تجيب القيمة من الكاش أولاً
			var cached = _cacheService.Get<string>(cacheKey);
			if (cached != null)
			{
				return Ok(new { source = "cache", value = cached });
			}

			// لو مش موجودة، خدها من "السيرفر" وحطها في الكاش
			string serverValue = "Hello from server! " + DateTime.Now.ToString("HH:mm:ss");
			_cacheService.Set(cacheKey, serverValue);

			return Ok(new { source = "server", value = serverValue });
		}
	}
}
