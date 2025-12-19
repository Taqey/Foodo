using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Product;
using Foodo.Application.Models.Enums;
using Foodo.Domain.Enums;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Products
{
	public interface ICustomerProductsReadService
	{
		Task<PaginationDto<CustomerProductDto>> ReadProducts(int Page, int PageSize, string? restaurantId, FoodCategory? categoryId, ProductOrderBy? orderBy, OrderingDirection? direction);
	}
}
