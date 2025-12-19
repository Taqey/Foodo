using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Orders;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Order;
using Foodo.Application.Models.Response;
using Foodo.Application.Queries.Orders.GetOrders.GetCustomerOrders;
using MediatR;

public class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery, ApiResponse<PaginationDto<OrderBaseDto>>>
{
	private readonly ICustomerOrdersReadService _service;

	public GetCustomerOrdersQueryHandler(ICustomerOrdersReadService service)
	{
		_service = service;
	}

	public async Task<ApiResponse<PaginationDto<OrderBaseDto>>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
	{
		var CustomerOrders = await _service.GetCustomerOrders(request.UserId, request.Page, request.PageSize);

		var baseOrders = new PaginationDto<OrderBaseDto>
		{
			TotalItems = CustomerOrders.TotalItems,
			TotalPages = CustomerOrders.TotalPages,
			Items = CustomerOrders.Items.Cast<OrderBaseDto>().ToList()
		};

		return ApiResponse<PaginationDto<OrderBaseDto>>.Success(baseOrders);
	}
}
