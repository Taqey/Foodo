using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Product;
using Foodo.Application.Models.Dto.Product;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Products.GetProduct.GetCustomerProduct
{
	public class GetCustomerProductQueryHandler : IRequestHandler<GetCustomerProductQuery, ApiResponse<ProductBaseDto>>
	{
		private readonly ICustomerProductReadService _service;

		public GetCustomerProductQueryHandler(ICustomerProductReadService service)
		{
			_service = service;
		}
		public async Task<ApiResponse<ProductBaseDto>> Handle(GetCustomerProductQuery request, CancellationToken cancellationToken)
		{
			var result = await _service.ReadProduct(request.productId);
			if (result == null)
			{
			return	ApiResponse<ProductBaseDto>.Failure("No such Product");
			}
			return ApiResponse<ProductBaseDto>.Success(result);
		}
	}
}
