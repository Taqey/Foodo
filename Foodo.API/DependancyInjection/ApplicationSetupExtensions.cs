using Foodo.Domain.Entities;
using Foodo.Infrastructure.Helper;
using Foodo.Infrastructure.Perisistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Reflection;
using System.Text;

namespace Foodo.API.Extensions
{
	public static class ApplicationSetupExtensions
	{
		public static IServiceCollection AddApplicationConfigurations(this IServiceCollection services, IConfiguration configuration)
		{


			// JWT & Identity
			services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
			services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));
			services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateLifetime = true,
						ValidateIssuerSigningKey = true,
						ClockSkew = TimeSpan.Zero,
						ValidAudience = configuration["Jwt:Audience"],
						ValidIssuer = configuration["Jwt:Issuer"],
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
					};
				});

			services.AddIdentityCore<ApplicationUser>(options =>
			{
				options.Password.RequireDigit = true;
				options.Password.RequireLowercase = true;
				options.Password.RequireUppercase = true;
				options.Password.RequireNonAlphanumeric = true;
				options.Password.RequiredLength = 8;
			})
			.AddRoles<IdentityRole>()
			.AddEntityFrameworkStores<AppDbContext>()
			.AddDefaultTokenProviders();

			// Swagger
			services.AddSwaggerGen(options =>
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
					[new OpenApiSecuritySchemeReference("bearer", document)] = new List<string>()
				});
			});

			services.AddHttpContextAccessor();
			services.AddProblemDetails();

			return services;
		}
	}
}
