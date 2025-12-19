using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Products;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Product;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Products.GetProducts.GetMerchantProducts
{
	public class GetMerchantProductsQueryHandler : IRequestHandler<GetMerchantProductsQuery, ApiResponse<PaginationDto<ProductBaseDto>>>
	{
		private readonly IMerchantProductsReadService _service;

		public GetMerchantProductsQueryHandler(IMerchantProductsReadService service)
		{
			_service = service;
		}
		public async Task<ApiResponse<PaginationDto<ProductBaseDto>>> Handle(GetMerchantProductsQuery request, CancellationToken cancellationToken)
		{
			var result = await _service.ReadProducts(request.Page, request.PageSize, request.restaurantId, request.categoryId, request.orderBy, request.orderingDirection);

			var baseProducts = new PaginationDto<ProductBaseDto>
			{
				TotalItems = result.TotalItems,
				TotalPages = result.TotalPages,
				Items = result.Items.Cast<ProductBaseDto>().ToList()
			};

			return ApiResponse<PaginationDto<ProductBaseDto>>.Success(baseProducts);


		}
	}
}
