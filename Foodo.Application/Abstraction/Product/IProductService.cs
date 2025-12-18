using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Response;

namespace Foodo.Application.Abstraction.Product
{
	public interface IProductService<Tdto>
	{
		Task<ApiResponse<Tdto>> ReadProduct(ItemByIdInput input);
		Task<ApiResponse<PaginationDto<Tdto>>> ReadProducts(ProductPaginationInput input);

	}
}
