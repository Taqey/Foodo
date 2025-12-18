using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Product;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Response;

namespace Foodo.Application.Factory.Product
{
	public interface IProductStrategy
	{
		Task<ApiResponse<ProductBaseDto>> ReadProduct(ItemByIdInput input);
		Task<ApiResponse<PaginationDto<ProductBaseDto>>> ReadProducts(ProductPaginationInput input);
	}
}
