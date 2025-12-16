using Foodo.Application.Models.Dto.Order;
using Foodo.Application.Models.Input.Merchant;
using Foodo.Application.Models.Response;

namespace Foodo.Application.Abstraction.Orders
{
	public interface IMerchantOrderService : IOrderService<MerchantOrderDto>
	{
		Task<ApiResponse> UpdateOrderStatusAsync(int orderId, OrderStatusUpdateInput input);

	}
}
