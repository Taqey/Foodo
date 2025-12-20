using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Customers;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Customers
{
	public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, ApiResponse<PaginationDto<CustomerDto>>>
	{
		private readonly ICustomersReadService _service;

		public GetCustomersQueryHandler(ICustomersReadService service)
		{
			_service = service;
		}
		public async Task<ApiResponse<PaginationDto<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
		{
			var result = await _service.ReadCustomers(request.RestaurantId, request.Page, request.PageSize);
			return ApiResponse<PaginationDto<CustomerDto>>.Success(result);
		}
	}
}
