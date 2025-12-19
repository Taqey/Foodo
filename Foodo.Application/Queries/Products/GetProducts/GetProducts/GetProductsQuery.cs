using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Product;
using Foodo.Application.Models.Enums;
using Foodo.Application.Models.Response;
using Foodo.Domain.Enums;
using MediatR;

namespace Foodo.Application.Queries.Products.GetProducts.GetProducts
{
	public class GetProductsQuery : IRequest<ApiResponse<PaginationDto<ProductBaseDto>>>
	{
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public string? restaurantId { get; set; }
		public FoodCategory? categoryId { get; set; }
		public ProductOrderBy? orderBy { get; set; }
		public OrderingDirection? orderingDirection { get; set; }
	}
}
