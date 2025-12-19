using Foodo.Application.Models.Dto.Profile.Customer;
using Foodo.Application.Models.Response;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Queries.Profile.GetCustomerProfile
{
	public class GetCustomerProfileQueryHandler : IRequestHandler<GetCustomerProfileQuery, ApiResponse<CustomerProfileDto>>
	{
		public Task<ApiResponse<CustomerProfileDto>> Handle(GetCustomerProfileQuery request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
