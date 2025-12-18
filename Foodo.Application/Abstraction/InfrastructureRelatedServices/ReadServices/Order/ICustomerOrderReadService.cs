using Foodo.Application.Models.Dto.Order;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Order
{
	public interface ICustomerOrderReadService
	{
		Task<CustomerOrderDto> GetCustomerOrder(int OrderId);

	}
}
