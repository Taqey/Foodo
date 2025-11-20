
using Foodo.Application.Abstraction;
using Foodo.Application.Implementation;
using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using Foodo.Infrastructure.Helper;
using Foodo.Infrastructure.Perisistence;
using Foodo.Infrastructure.Repository;
using Foodo.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Reflection;
using System.Text;
namespace Foodo.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

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
					Title = "Food API",
					Description = "A sample application with Swagger, Swashbuckle, and API versioning for managing Foods",
					TermsOfService = new Uri("https://example.com/terms"),
					Contact = new OpenApiContact
					{
						Name = "Example Contact",
						Url = new Uri("https://example.com/contact")
					},
					License = new OpenApiLicense
					{
						Name = "Example License",
						Url = new Uri("https://example.com/license")
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
					ValidateIssuer = false,
					ValidateAudience = false,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = false,
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
			builder.Services.AddScoped(typeof(IRepository<>),typeof(Repository<>));
			builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
			builder.Services.AddScoped<IUserService, UserService>();
			builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
			builder.Services.AddScoped<ICreateToken, CreateToken>();
			builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
			builder.Services.AddHttpContextAccessor();
			builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
			builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));


			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.MapOpenApi();
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();
			app.UseAuthentication();

			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
