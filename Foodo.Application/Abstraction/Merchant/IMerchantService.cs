using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Abstraction.Merchant;

public interface IMerchantService
{
	Task<ApiResponse> CreateProductAsync(ProductInput request);
	Task<ApiResponse<ProductDto>> ReadProductByIdAsync(int productId);
	Task<ApiResponse<PaginationDto<ProductDto>>> ReadAllProductsAsync(ProductPaginationInput input);
	Task<ApiResponse> UpdateProductAsync(ProductUpdateInput input);
	Task<ApiResponse> DeleteProductAsync(int productId);
	Task<ApiResponse> AddProductAttributeAsync(int productDetailId, AttributeCreateInput attributes);
	Task<ApiResponse> RemoveProductAttributeAsync(int productDetailId, AttributeDeleteInput attributes);
	Task<ApiResponse<PaginationDto<MerchantOrderDto>>> ReadAllOrdersAsync(ProductPaginationInput input);
	Task <ApiResponse<MerchantOrderDto>> ReadOrderByIdAsync(int orderId);
	Task <ApiResponse> UpdateOrderStatusAsync(int orderId, OrderStatusUpdateInput input);
	Task<ApiResponse<PaginationDto<CustomerDto>>> ReadAllPurchasedCustomersAsync(ProductPaginationInput input);
	Task<ApiResponse> AddProductCategoriesAsync(ProductCategoryInput categoryInput);
	Task<ApiResponse> RemoveProductCategoriesAsync(ProductCategoryInput categoryInput);
}
