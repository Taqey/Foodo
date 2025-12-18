using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Order;
using Foodo.Application.Models.Dto.Order;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Orders.GetOrder.GetMerchantOrder
{
	public class GetMerchantOrderQueryHandler : IRequestHandler<GetMerchantOrderQuery, ApiResponse<OrderBaseDto>>
	{
		private readonly IMerchantOrderReadService _service;

		public GetMerchantOrderQueryHandler(IMerchantOrderReadService service)
		{
			_service = service;
		}
		public async Task<ApiResponse<OrderBaseDto>> Handle(GetMerchantOrderQuery request, CancellationToken cancellationToken)
		{
			var result = await _service.GetMerchantOrder(request.OrderId);
			if (result == null)
			{
				return ApiResponse<OrderBaseDto>.Failure("there's no such Id");
			}
			return ApiResponse<OrderBaseDto>.Success(result);
		}
	}
}
