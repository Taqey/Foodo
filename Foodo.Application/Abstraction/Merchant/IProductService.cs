using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Abstraction.Merchant;

public interface IProductService
{
	Task<ApiResponse> CreateProductAsync(ProductInput request);
	Task<ApiResponse<ProductDto>> ReadProductByIdAsync(int productId);
	Task<ApiResponse<List<ProductDto>>> ReadAllProductsAsync(string UserId);
	Task<ApiResponse> UpdateProductAsync(int productId, ProductInput request);
	Task<ApiResponse> DeleteProductAsync(int productId);
	Task<ApiResponse> AddProductAttributeAsync(int productDetailId, AttributeCreateInput attributes);
	Task<ApiResponse> RemoveProductAttributeAsync(int productDetailId, AttributeDeleteInput attributes);

}
