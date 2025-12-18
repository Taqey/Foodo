using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Order;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Orders
{
	public interface ICustomerOrdersReadService
	{
		Task<PaginationDto<CustomerOrderDto>> GetCustomerOrders(string customerId, int page, int pageSize);
	}
}
