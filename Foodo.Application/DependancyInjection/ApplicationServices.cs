using Foodo.Application.Abstraction.Customer;
using Foodo.Application.Abstraction.Merchant;
using Foodo.Application.Abstraction.Photo;
using Foodo.Application.Abstraction.Profile.CustomerProfile;
using Foodo.Application.Abstraction.Profile.MerchantProfile;
using Foodo.Application.Factory.Address;
using Foodo.Application.Factory.Order;
using Foodo.Application.Factory.Product;
using Foodo.Application.Implementation.Customer;
using Foodo.Application.Implementation.Merchant;
using Foodo.Application.Implementation.Photo;
using Foodo.Application.Implementation.Profile.CustomerProfile;
using Foodo.Application.Implementation.Profile.MerchantProfile;

using Microsoft.Extensions.DependencyInjection;

namespace Foodo.API.Extensions
{
	public static class ApplicationServicesExtensions
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{
			services.AddScoped<IMerchantService, MerchantService>();
			services.AddScoped<ICustomerService, CustomerService>();
			services.AddScoped<IPhotoService, PhotoService>();

			services.AddScoped<IMerchantAdressService, MerchantAdressService>();
			services.AddScoped<ICustomerAdressService, CustomerAdressService>();

			services.AddScoped<IAddressStrategyFactory, AddressStrategyFactory>();
			services.AddScoped<IOrderStrategyFactory, OrderStrategyFactory>();
			services.AddScoped<IProductStrategyFactory, ProductStrategyFactory>();


			return services;
		}
	}
}
