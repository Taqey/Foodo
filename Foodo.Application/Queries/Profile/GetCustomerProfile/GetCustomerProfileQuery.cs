using Foodo.Application.Models.Dto.Profile.Customer;
using Foodo.Application.Models.Response;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Queries.Profile.GetCustomerProfile
{
	public class GetCustomerProfileQuery:IRequest<ApiResponse<CustomerProfileDto>>
	{
		public string UserId { get; set; }

	}
}
