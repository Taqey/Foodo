using Foodo.Application.Models.Dto.Order;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Order
{
	public interface IMerchantOrderReadService
	{
		Task<MerchantOrderDto> GetMerchantOrder(int OrderId);

	}
}
