using Foodo.Application.Factory.Address;
using Foodo.Application.Factory.Order;
using Foodo.Application.Factory.Product;

using Microsoft.Extensions.DependencyInjection;

namespace Foodo.API.Extensions
{
	public static class ApplicationServicesExtensions
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{


			services.AddScoped<IAddressStrategyFactory, AddressStrategyFactory>();
			services.AddScoped<IOrderStrategyFactory, OrderStrategyFactory>();
			services.AddScoped<IProductStrategyFactory, ProductStrategyFactory>();


			return services;
		}
	}
}
