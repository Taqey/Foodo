using Foodo.Application.Models.Dto.Order;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Response;

namespace Foodo.Application.Abstraction.Orders
{
	public interface ICustomerOrderService :IOrderService<CustomerOrderDto>
	{
		Task<ApiResponse> PlaceOrder(CreateOrderInput input);
		Task<ApiResponse> CancelOrder(ItemByIdInput input);
	}
}
