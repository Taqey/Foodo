using Foodo.Application.Abstraction.InfrastructureRelatedServices.Authentication;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.Mailing;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Customers;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Order;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Orders;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Photos;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Product;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Products;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Profile.CustomerProfile;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Profile.MerchantProfile;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Restaurant;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Restaurants;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.Upload;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.User;
using Foodo.Domain.Repository;
using Foodo.Infrastructure.Repository;
using Foodo.Infrastructure.Services.Authentication;
using Foodo.Infrastructure.Services.Mailing;
using Foodo.Infrastructure.Services.ReadServices.Order;
using Foodo.Infrastructure.Services.ReadServices.Photos;
using Foodo.Infrastructure.Services.ReadServices.Product;
using Foodo.Infrastructure.Services.ReadServices.Products;
using Foodo.Infrastructure.Services.ReadServices.Profile.MerchantProfile;
using Foodo.Infrastructure.Services.Upload;
using Foodo.Infrastructure.Services.UserManagment;
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
			services.AddScoped<ICustomerProductReadService, CustomerProductReadService>();
			services.AddScoped<IMerchantProductReadService, MerchantProductReadService>();
			services.AddScoped<ICustomerProductsReadService, CustomerProductsReadService>();
			services.AddScoped<IMerchantProductsReadService, MerchantProductsReadService>();
			services.AddScoped<ICustomerProfileReadService, CustomerProfileReadService>();
			services.AddScoped<IMerchantProfileReadService, MerchantProfileReadService>();
			services.AddScoped<IUserPhotoReadService, UserPhotoReadService>();
			services.AddScoped<IRestaurantReadService, RestaurantReadService>();
			services.AddScoped<IRestaurantsReadService, RestaurantsReadService>();
			services.AddScoped<ICustomersReadService, CustomersReadService>();

			return services;
		}
	}
}
