using Foodo.API.Extensions;
using Foodo.API.Middlewares;
using Foodo.Infrastructure.Perisistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
namespace Foodo.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			 Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Information()
			.Enrich.FromLogContext()
			.Enrich.WithMachineName()
			.Enrich.WithThreadName()
			.WriteTo.Console()
			.WriteTo.Seq("http://localhost:5341")
			.WriteTo.File("logs/foodo_log.txt", rollingInterval: RollingInterval.Day)
			.CreateLogger();

			builder.Host.UseSerilog();
			builder.Services.AddApplicationServices();
			builder.Services.AddInfrastructureServices();
			builder.Services.AddApplicationConfigurations(builder.Configuration);
			builder.Services.AddCachingAndRateLimiter(builder.Configuration);
			builder.Services.AddOpenApi();
			builder.Services.AddDbContext<AppDbContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
			// CORS Configuration
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowFrontend", builder =>
				{
					builder.SetIsOriginAllowed(origin =>
					{
						// Allow GitHub Pages (any sub-path)
						if (origin.StartsWith("https://taqey.github.io")) return true;

						// Allow localhost (any port)
						if (origin.StartsWith("http://localhost:") ||
							origin.StartsWith("http://127.0.0.1:")) return true;

						// Allow file:// protocol
						if (origin == "null") return true;

						return false;
					})
					.AllowAnyHeader()
					.AllowAnyMethod()
					.AllowCredentials();
				});
			});
			builder.Services.AddControllers();


			var app = builder.Build();

			app.UseRouting();
			app.UseGlobalExceptionHandlerMiddleware();
			app.UseSerilogRequestLogging();
			app.UseCors("AllowFrontend");
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseRateLimiter();
			app.UseSwagger();
			app.UseSwaggerUI();
			app.UseHttpsRedirection();
			app.UseMiddleware<PayloadSizeCheckMiddleware>(1024 * 50);
			app.MapControllers();
			app.Run();

		}
	}
}
