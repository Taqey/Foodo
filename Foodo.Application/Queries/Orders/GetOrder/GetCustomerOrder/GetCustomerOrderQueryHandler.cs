using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Order;
using Foodo.Application.Models.Dto.Order;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Orders.GetOrder.GetCustomerOrder
{
	public class GetCustomerOrderQueryHandler : IRequestHandler<GetCustomerOrderQuery, ApiResponse<OrderBaseDto>>
	{
		private readonly ICustomerOrderReadService _service;

		public GetCustomerOrderQueryHandler(ICustomerOrderReadService service)
		{
			_service = service;
		}
		public async Task<ApiResponse<OrderBaseDto>> Handle(GetCustomerOrderQuery request, CancellationToken cancellationToken)
		{
			var result = await _service.GetCustomerOrder(request.OrderId);
			if (result == null)
			{
				return ApiResponse<OrderBaseDto>.Failure("there's no such Id");
			}
			return ApiResponse<OrderBaseDto>.Success(result);
		}
	}
}
