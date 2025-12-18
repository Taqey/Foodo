using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Orders;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Order;
using Foodo.Application.Models.Response;
using Foodo.Application.Queries.Orders.GetOrders.GetMerchantOrders;
using MediatR;

public class GetMerchantOrdersQueryHandler : IRequestHandler<GetMerchantOrdersQuery, ApiResponse<PaginationDto<OrderBaseDto>>>
{
	private readonly IMerchantOrdersReadService _service;

	public GetMerchantOrdersQueryHandler(IMerchantOrdersReadService service)
	{
		_service = service;
	}

	public async Task<ApiResponse<PaginationDto<OrderBaseDto>>> Handle(GetMerchantOrdersQuery request, CancellationToken cancellationToken)
	{
		var merchantOrders = await _service.GetMerchantOrders(request.UserId, request.Page, request.PageSize);

		var baseOrders = new PaginationDto<OrderBaseDto>
		{
			TotalItems = merchantOrders.TotalItems,
			TotalPages = merchantOrders.TotalPages,
			Items = merchantOrders.Items.Cast<OrderBaseDto>().ToList()
		};

		return ApiResponse<PaginationDto<OrderBaseDto>>.Success(baseOrders);
	}
}
