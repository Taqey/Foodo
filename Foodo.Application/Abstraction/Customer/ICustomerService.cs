using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Response;

namespace Foodo.Application.Abstraction.Customer
{
	public interface ICustomerService
	{
		Task<ApiResponse<PaginationDto<ShopDto>>> ReadAllShops(ProductPaginationInput input);
		Task<ApiResponse<PaginationDto<ShopDto>>> ReadShopsByCategory(ShopsPaginationByCategoryInput input);

		Task<ApiResponse<ShopDto>> ReadShopById(ItemByIdInput input);

	}
}
