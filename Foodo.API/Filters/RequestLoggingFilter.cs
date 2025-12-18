using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace Foodo.API.Filters
{
	public class RequestLoggingFilter : IActionFilter
	{
		public void OnActionExecuted(ActionExecutedContext context)
		{
			var http = context.HttpContext;

			Log.Information(
				"Request finished | StatusCode={StatusCode} | TraceId={TraceId}",
				http.Response.StatusCode,
				http.TraceIdentifier
			);
		}

		public void OnActionExecuting(ActionExecutingContext context)
		{
			var http = context.HttpContext;

			Log.Information(
				"Request started | {Method} {Path} | TraceId={TraceId}",
				http.Request.Method,
				http.Request.Path,
				http.TraceIdentifier
			);
		}
	}
}
