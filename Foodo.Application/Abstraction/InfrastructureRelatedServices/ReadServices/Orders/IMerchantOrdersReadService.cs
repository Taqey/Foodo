using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Order;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Orders
{
	public interface IMerchantOrdersReadService
	{
		Task<PaginationDto<MerchantOrderDto>> GetMerchantOrders(string merchantId, int page, int pageSize);

	}
}
