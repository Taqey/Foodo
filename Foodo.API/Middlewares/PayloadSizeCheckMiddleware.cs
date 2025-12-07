using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Crypto.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Foodo.API.Middlewares
{
	// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
	public class PayloadSizeCheckMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly int _size;
		public PayloadSizeCheckMiddleware(RequestDelegate next, int size)
		{
			_next = next;
			_size = size;
		}

		public async Task Invoke(HttpContext httpContext)
		{
			var OriginalBody=httpContext.Response.Body;
			using var memory=new MemoryStream();
			httpContext.Response.Body = memory;
			await _next(httpContext);
			memory.Seek(0, SeekOrigin.Begin);
			if (memory.Length>=_size)
			{
				httpContext.Response.Headers.Add("Content-Encoding", "gzip");
				using var gzip=new GZipStream(OriginalBody, CompressionMode.Compress,leaveOpen:true);
				await memory.CopyToAsync(gzip);
			}
			else
			{
				memory.Seek(0, SeekOrigin.Begin);
				await memory.CopyToAsync(OriginalBody);
			}
		}
	}

	// Extension method used to add the middleware to the HTTP request pipeline.
	public static class PayloadSizeCheckMiddlewareExtensions
	{
		public static IApplicationBuilder UsePayloadSizeCheckMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<PayloadSizeCheckMiddleware>();
		}
	}
}
