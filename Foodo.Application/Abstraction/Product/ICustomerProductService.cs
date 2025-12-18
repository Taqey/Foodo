using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Product;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Response;

namespace Foodo.Application.Abstraction.Product
{
	public interface ICustomerProductService : IProductService<CustomerProductDto>
	{
		Task<ApiResponse<PaginationDto<CustomerProductDto>>> ReadProductsByCategory(ProductPaginationByCategoryInput input);
		Task<ApiResponse<PaginationDto<CustomerProductDto>>> ReadProductsByShop(ProductPaginationByShopInput input);
	}
}
