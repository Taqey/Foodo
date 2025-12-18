using Foodo.Application.Abstraction.Authentication;
using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Abstraction.InfrastructureRelatedServices;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Order;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Orders;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Product;
using Foodo.Domain.Repository;
using Foodo.Infrastructure.Repository;
using Foodo.Infrastructure.Services;
using Foodo.Infrastructure.Services.ReadServices.Order;
using Foodo.Infrastructure.Services.ReadServices.Product;
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
			services.AddScoped<ICustomerOrdersReadService, CustomerOrdersReadService>();
			services.AddScoped<IMerchantOrdersReadService, MerchantOrdersReadService>();
			services.AddScoped<ICustomerOrderReadService, CustomerOrderReadService>();
			services.AddScoped<IMerchantOrderReadService, MerchantOrderReadService>();
			services.AddScoped<ICustomerProductReadService,CustomerProductReadService>();
			services.AddScoped<IMerchantProductReadService, MerchantProductReadService>();
			return services;
		}
	}
}
