using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Product;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Abstraction.Product
{
	public interface ICustomerProductService :IProductService<CustomerProductDto>
	{
		Task<ApiResponse<PaginationDto<CustomerProductDto>>> ReadProductsByCategory(ProductPaginationByCategoryInput input);
		Task<ApiResponse<PaginationDto<CustomerProductDto>>> ReadProductsByShop(ProductPaginationByShopInput input);
	}
}
