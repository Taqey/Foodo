using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;


namespace Foodo.API.Filters
{
	public class ValidateIdFilter : IActionFilter
	{
		public void OnActionExecuted(ActionExecutedContext context)
		{
		}

		public void OnActionExecuting(ActionExecutingContext context)
		{
			if (context.ActionArguments.TryGetValue("id", out var value) && value is int id && id <= 0)
			{
				Log.Warning("Invalid Id provided | Id={Id} | {Method} {Path} | TraceId={TraceId}",
					id,
					context.HttpContext.Request.Method,
					context.HttpContext.Request.Path,
					context.HttpContext.TraceIdentifier
				);
				context.Result = new BadRequestObjectResult(new
				{
					message = "Invalid ID",
					traceId = context.HttpContext.TraceIdentifier
				});
			}
		}
	}
}
