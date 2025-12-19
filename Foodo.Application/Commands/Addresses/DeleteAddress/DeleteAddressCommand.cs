using Foodo.Application.Models.Response;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Commands.Addresses.DeleteAddress
{
	public class DeleteAddressCommand :IRequest<ApiResponse>
	{
		public string userId { get; set; }
		public int adressId { get; set; }
	}
}
