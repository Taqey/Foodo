using Foodo.Application.Models.Response;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Commands.Addresses.SetAddressDefault
{
	public class SetAddressDefaultCommand :IRequest <ApiResponse>
	{
		public string CustomerId { get; set; }
		public int AdressId { get; set; }
	}
}
