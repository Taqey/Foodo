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
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
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
			builder.Services.AddControllers();
			// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
			builder.Services.AddOpenApi();
			builder.Services.AddDbContext<AppDbContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
			builder.Services.AddSwaggerGen(options =>
			{


				options.SwaggerDoc("v1", new OpenApiInfo
				{
					Version = "v1",
					Title = "Foodo API",
					Description = "Foodo API provides authentication, user management, merchant onboarding, and food ordering functionalities.",
					TermsOfService = new Uri("https://foodo.com/terms"),
					Contact = new OpenApiContact
					{
						Name = "Taqeyy Eldeen",
						Email = "atakieeldeen@gmail.com",
						Url = new Uri("https://bucolic-cobbler-83dcdd.netlify.app/")
					},
					License = new OpenApiLicense
					{
						Name = "Foodo API License",
						Url = new Uri("https://foodo.com/license")
					}
				});

				var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
				options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
				{
					Type = SecuritySchemeType.Http,
					Scheme = "bearer",
					BearerFormat = "JWT",
					Description = "JWT Authorization header using the Bearer scheme."
				});
				options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
				{
					[new OpenApiSecuritySchemeReference("bearer", document)] = []
				});


			});
			builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ClockSkew = TimeSpan.Zero,
					ValidAudience = builder.Configuration["Jwt:Audience"],
					ValidIssuer = builder.Configuration["Jwt:Issuer"],
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))


				};
			});
			builder.Services.AddIdentityCore<ApplicationUser>(options =>
			{
				options.Password.RequireDigit = true;
				options.Password.RequireLowercase = true;
				options.Password.RequireUppercase = true;
				options.Password.RequireNonAlphanumeric = true;
				options.Password.RequiredLength = 8;

			}).AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
			builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
			builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
			builder.Services.AddScoped<IUserService, UserService>();
			builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
			builder.Services.AddScoped<ICreateToken, CreateToken>();
			builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
			builder.Services.AddScoped<IMerchantService, MerchantService>();
			builder.Services.AddScoped<ICustomerService, CustomerService>();
			builder.Services.AddScoped<IMerchantProfileService, MerchantProfileService>();
			builder.Services.AddScoped<ICustomerProfileService, CustomerProfileService>();
			builder.Services.AddSingleton<IPhotoAccessorService, PhotoAccessorService>();
			builder.Services.AddScoped<IPhotoService, PhotoService>();
			builder.Services.AddScoped<IProductRepository, ProductRepository>();
			builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
			builder.Services.AddScoped<IUserRepository, UserRepository>();
			builder.Services.AddScoped<IOrderRepository, OrderRepository>();
			builder.Services.AddScoped<IProductPhotoCustomRepository, ProductPhotoCustomRepository>();
			builder.Services.AddHttpContextAccessor();
			builder.Services.AddProblemDetails();
			builder.Services.AddMemoryCache();
			builder.Services.AddRateLimiter(options =>
			{
				options.AddFixedWindowLimiter("FixedPolicy", opt =>
				{
					opt.Window = TimeSpan.FromMinutes(1);
					opt.PermitLimit = 100;
					opt.QueueLimit = 2;
					opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;

				});
			});
			builder.Services.AddSingleton<ICacheService, CacheService>();
			builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
			builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
			builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));

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


			var app = builder.Build();
			app.UseRateLimiter();
			app.UseSerilogRequestLogging();
			app.UseGlobalExceptionHandlerMiddleware();
			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.MapOpenApi();
			}
			app.UseSwagger();
			app.UseSwaggerUI();
			app.UseHttpsRedirection();
			app.UseCors("AllowFrontend");
			app.UseMiddleware<PayloadSizeCheckMiddleware>(1024 * 50);
			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();
			app.MapControllers();
			app.Run();
		}
	}
}
