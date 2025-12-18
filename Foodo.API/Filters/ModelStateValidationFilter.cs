using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace Foodo.API.Filters
{
	public class ModelStateValidationFilter : IActionFilter
	{
		public void OnActionExecuting(ActionExecutingContext context)
		{
			if (!context.ModelState.IsValid)
			{
				var errors = string.Join(" | ",
					context.ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage));

				var path = context.HttpContext.Request.Path;
				var method = context.HttpContext.Request.Method;

				var traceId = context.HttpContext.TraceIdentifier;

				Log.Warning(
					"Validation failed | {Method} {Path} | Errors={Errors} | TraceId={TraceId}",
					method,
					path,

					errors,
					traceId
				);

				context.Result = new BadRequestObjectResult(new
				{
					message = "Invalid request data",
					errors,
					traceId
				});
			}
		}

		public void OnActionExecuted(ActionExecutedContext context)
		{
		}
	}
}
