using Foodo.Application.Models.Dto.Merchant;
using Foodo.Application.Models.Dto.Product;
using Foodo.Application.Models.Input.Merchant;
using Foodo.Application.Models.Response;

namespace Foodo.Application.Abstraction.Product
{
	public interface IMerchantProductService : IProductService<MerchantProductDto>
	{
		Task<ApiResponse<CreateProductDto>> CreateProductAsync(ProductInput request);
		Task<ApiResponse> UpdateProductAsync(ProductUpdateInput input);
		Task<ApiResponse> DeleteProductAsync(int productId);
		Task<ApiResponse> AddProductAttributeAsync(int productId, AttributeCreateInput attributes);
		Task<ApiResponse> RemoveProductAttributeAsync(int productId, AttributeDeleteInput attributes);
		Task<ApiResponse> AddProductCategoriesAsync(ProductCategoryInput categoryInput);
		Task<ApiResponse> RemoveProductCategoriesAsync(ProductCategoryInput categoryInput);
	}
}
