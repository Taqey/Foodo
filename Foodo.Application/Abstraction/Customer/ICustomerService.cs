using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Response;

namespace Foodo.Application.Abstraction.Customer
{
	public interface ICustomerService
	{
		Task<ApiResponse<ProductDto>> ReadProductById(ItemByIdInput input);
		Task<ApiResponse<PaginationDto<ProductDto>>> ReadAllProducts(ProductPaginationInput input);
		Task<ApiResponse<PaginationDto<ProductDto>>> ReadProductsByCategory(ProductPaginationByCategoryInput input);
		Task<ApiResponse<PaginationDto<ProductDto>>> ReadProductsByShop(ProductPaginationByShopInput input);
		Task<ApiResponse<PaginationDto<ShopDto>>> ReadAllShops(ProductPaginationInput input);
		Task<ApiResponse<PaginationDto<ShopDto>>> ReadShopsByCategory(ShopsPaginationByCategoryInput input);

		Task<ApiResponse<ShopDto>> ReadShopById(ItemByIdInput input);
		Task<ApiResponse<PaginationDto<CustomerOrderDto>>> ReadAllOrders(ProductPaginationInput input);
		Task<ApiResponse<CustomerOrderDto>> ReadOrderById(ItemByIdInput input);
		Task<ApiResponse> PlaceOrder(CreateOrderInput input);
		Task<ApiResponse> EditOrder();
		Task<ApiResponse> CancelOrder(ItemByIdInput input);
	}
}
