using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Application.Models.Dto.Order;
using Foodo.Application.Models.Dto.Product;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Response;

namespace Foodo.Application.Abstraction.Customer
{
	public interface ICustomerService
	{
		Task<ApiResponse<CustomerProductDto>> ReadProductById(ItemByIdInput input);
		Task<ApiResponse<PaginationDto<CustomerProductDto>>> ReadAllProducts(ProductPaginationInput input);
		Task<ApiResponse<PaginationDto<CustomerProductDto>>> ReadProductsByCategory(ProductPaginationByCategoryInput input);
		Task<ApiResponse<PaginationDto<CustomerProductDto>>> ReadProductsByShop(ProductPaginationByShopInput input);


		Task<ApiResponse<PaginationDto<ShopDto>>> ReadAllShops(ProductPaginationInput input);
		Task<ApiResponse<PaginationDto<ShopDto>>> ReadShopsByCategory(ShopsPaginationByCategoryInput input);

		Task<ApiResponse<ShopDto>> ReadShopById(ItemByIdInput input);

	}
}
