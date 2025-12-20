using Foodo.Application.Models.Dto.Profile.Customer;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Profile.CustomerProfile
{
	public interface ICustomerProfileReadService
	{
		Task<CustomerProfileDto> ReadCustomerProfile(string UserId);
	}
}
