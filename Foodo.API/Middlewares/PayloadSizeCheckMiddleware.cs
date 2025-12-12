using System.IO.Compression;

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
			var originalBody = httpContext.Response.Body;

			await using var memory = new MemoryStream();
			httpContext.Response.Body = memory;

			try
			{
				await _next(httpContext);

				memory.Seek(0, SeekOrigin.Begin);

				if (memory.Length >= _size)
				{
					httpContext.Response.Headers.Add("Content-Encoding", "gzip");

					await using var gzip = new GZipStream(originalBody, CompressionMode.Compress, leaveOpen: true);
					await memory.CopyToAsync(gzip);
				}
				else
				{
					memory.Seek(0, SeekOrigin.Begin);
					await memory.CopyToAsync(originalBody);
				}
			}
			finally
			{
				httpContext.Response.Body = originalBody; // ✔ important
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
