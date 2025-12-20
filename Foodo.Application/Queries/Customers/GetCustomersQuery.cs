using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Customers
{
	public class GetCustomersQuery : IRequest<ApiResponse<PaginationDto<CustomerDto>>>
	{
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public string? RestaurantId { get; set; }
	}
}
