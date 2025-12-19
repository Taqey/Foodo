using Foodo.Application.Models.Dto.Product;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Product
{
	public interface ICustomerProductReadService
	{
		Task<CustomerProductDto> ReadProduct(int productId);
	}
}
