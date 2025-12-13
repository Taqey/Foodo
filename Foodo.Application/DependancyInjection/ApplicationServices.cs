using Foodo.Application.Abstraction.Authentication;
using Foodo.Application.Abstraction.Customer;
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
using Microsoft.Extensions.DependencyInjection;

namespace Foodo.API.Extensions
{
	public static class ApplicationServicesExtensions
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{
			services.AddScoped<IMerchantService, MerchantService>();
			services.AddScoped<ICustomerService, CustomerService>();
			services.AddScoped<IMerchantAdressService, MerchantAdressService>();
			services.AddScoped<ICustomerAdressService, CustomerAdressService>();
			services.AddScoped<IAuthenticationService, AuthenticationService>();
			services.AddScoped<IPhotoService, PhotoService>();

			return services;
		}
	}
}
