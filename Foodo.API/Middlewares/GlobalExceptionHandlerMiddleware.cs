using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics;

namespace Foodo.API.Middlewares
{
	public class GlobalExceptionHandlerMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IHostEnvironment _env;

		public GlobalExceptionHandlerMiddleware(RequestDelegate next, IHostEnvironment env)
		{
			_next = next;
			_env = env;
		}

		public async Task Invoke(HttpContext httpContext)
		{
			var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

			try
			{
				await _next(httpContext);
			}
			catch (Exception ex)
			{
				if (httpContext.Response.HasStarted)
				{
					Log.Warning("Response already started, cannot handle exception.");
					throw;
				}

				//httpContext.Response.Clear();

				int statusCode;
				ProblemDetails problem;

				// Server errors
				if (ex is NullReferenceException ||
					ex is InvalidOperationException ||
					ex is Microsoft.Data.SqlClient.SqlException)
				{
					statusCode = StatusCodes.Status500InternalServerError;
					problem = new ProblemDetails
					{
						Title = "Internal Server Error",
						Status = statusCode,
						Instance = httpContext.Request.Path,
						Detail = _env.IsDevelopment() ? ex.Message : "An unexpected error occurred."
					};

					Log.Fatal(ex, "Unhandled Server error | Path: {Path} | TraceId: {TraceId}", httpContext.Request.Path, traceId);
				}
				else
				{
					statusCode = StatusCodes.Status400BadRequest;
					problem = new ProblemDetails
					{
						Title = "Bad Request",
						Status = statusCode,
						Instance = httpContext.Request.Path,
						Detail = ex.Message
					};

					Log.Error(ex, "Unhandled Client error | Path: {Path} | TraceId: {TraceId}", httpContext.Request.Path, traceId);
				}

				httpContext.Response.StatusCode = statusCode;
				httpContext.Response.ContentType = "application/problem+json";

				await httpContext.Response.WriteAsJsonAsync(problem);
			}
		}
	}

	public static class GlobalExceptionHandlerMiddlewareExtensions
	{
		public static IApplicationBuilder UseGlobalExceptionHandlerMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
		}
	}
}
