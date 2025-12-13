using Foodo.Application.Abstraction.Authentication;
using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Abstraction.InfrastructureRelatedServices;
using Foodo.Domain.Repository;
using Foodo.Infrastructure.Repository;
using Foodo.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Foodo.API.Extensions
{
	public static class InfrastructureServicesExtensions
	{
		public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
		{
			services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
			services.AddScoped<IUnitOfWork, UnitOfWork>();
			services.AddScoped<IUserService, UserService>();
			services.AddScoped<ICreateToken, CreateToken>();
			services.AddScoped<IEmailSenderService, EmailSenderService>();
			services.AddScoped<IProductRepository, ProductRepository>();
			services.AddScoped<IRestaurantRepository, RestaurantRepository>();
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IOrderRepository, OrderRepository>();
			services.AddScoped<IProductPhotoCustomRepository, ProductPhotoCustomRepository>();
			services.AddSingleton<IPhotoAccessorService, PhotoAccessorService>();

			return services;
		}
	}
}
