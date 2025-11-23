using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;

namespace Foodo.Application.Abstraction.Customer
{
	public interface ICustomerService
	{
		Task<ApiResponse<ProductDto>> ReadProductById(ItemByIdInput input);
		Task<ApiResponse<IEnumerable<ProductDto>>> ReadAllProducts(PaginationInput input);
		Task<ApiResponse> ReadProductsByCategory();
		Task<ApiResponse<IEnumerable<ShopDto>>> ReadAllShops(PaginationInput input);
		Task<ApiResponse> ReadShopsByCategory();
		Task<ApiResponse<ShopDto>> ReadShopById(ItemByIdInput input);
		Task<ApiResponse<IEnumerable<OrderDto>>> ReadAllOrders(PaginationInput input);
		Task<ApiResponse<OrderDto>> ReadOrderById(ItemByIdInput input);
		Task<ApiResponse> PlaceOrder(CreateOrderInput input);
		Task<ApiResponse> EditOrder();
		Task<ApiResponse> CancelOrder(ItemByIdInput input);
	}
}
