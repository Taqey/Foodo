using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;

namespace Foodo.Application.Abstraction.Orders
{
	public interface IOrderService<TOrderDto>
	{
		Task<ApiResponse<TOrderDto>> GetOrderByIdAsync(int orderId);
		Task<ApiResponse<PaginationDto<TOrderDto>>> GetOrdersAsync(ProductPaginationInput input);

	}
}
