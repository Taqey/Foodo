using Foodo.Application.Models.Dto.Profile.Customer;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Profile.GetCustomerProfile
{
	public class GetCustomerProfileQuery : IRequest<ApiResponse<CustomerProfileDto>>
	{
		public string UserId { get; set; }

	}
}
