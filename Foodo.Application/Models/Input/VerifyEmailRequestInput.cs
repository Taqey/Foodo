using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Models.Input
{
	public class VerifyEmailRequestInput
	{
		public string Email { get; set; }
		public string Role { get; set; }
	}
}
