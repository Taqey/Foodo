using Foodo.Application.Models.Dto.Product;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Product
{
	public interface IMerchantProductReadService
	{
		Task<MerchantProductDto> ReadProduct(int productId);

	}
}
