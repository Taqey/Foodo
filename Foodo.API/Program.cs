using Foodo.API.Extensions;
using Foodo.API.Middlewares;
using Foodo.Application.Abstraction.Authentication;
using Foodo.Application.Abstraction.Customer;
using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Abstraction.InfrastructureRelatedServices;
using Foodo.Application.Abstraction.Merchant;
using Foodo.Application.Abstraction.Photo;
using Foodo.Application.Abstraction.Profile.CustomerProfile;
using Foodo.Application.Abstraction.Profile.MerchantProfile;
using Foodo.Application.Implementation.Authentication;
using Foodo.Application.Implementation.Customer;
using Foodo.Application.Implementation.Merchant;
using Foodo.Application.Implementation.Photo;
using Foodo.Application.Implementation.Profile.CustomerProfile;
using Foodo.Application.Implementation.Profile.MerchantProfile;
using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using Foodo.Infrastructure.Helper;
using Foodo.Infrastructure.Perisistence;
using Foodo.Infrastructure.Repository;
using Foodo.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using StackExchange.Redis;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;
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
