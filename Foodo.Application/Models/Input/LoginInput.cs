using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Foodo.Application.Models.Input
{
	public class LoginInput
	{

		public string Email { get; set; }
		public string Password { get; set; }
	}
}
