using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Customers
{
	public interface ICustomersReadService
	{
		Task<PaginationDto<CustomerDto>> ReadCustomers(string restaurantId, int page, int pageSize);
	}
}
