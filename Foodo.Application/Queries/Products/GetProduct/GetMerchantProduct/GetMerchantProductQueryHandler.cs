using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Product;
using Foodo.Application.Models.Dto.Product;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Products.GetProduct.GetMerchantProduct
{
	public class GetMerchantProductQueryHandler : IRequestHandler<GetMerchantProductQuery, ApiResponse<ProductBaseDto>>
	{
		private readonly IMerchantProductReadService _service;

		public GetMerchantProductQueryHandler(IMerchantProductReadService service)
		{
			_service = service;
		}
		public async Task<ApiResponse<ProductBaseDto>> Handle(GetMerchantProductQuery request, CancellationToken cancellationToken)
		{
			var result = await _service.ReadProduct(request.productId);
			if (result == null)
			{
				return ApiResponse<ProductBaseDto>.Failure("No such Product");
			}
			return ApiResponse<ProductBaseDto>.Success(result);
		}
	}
}
