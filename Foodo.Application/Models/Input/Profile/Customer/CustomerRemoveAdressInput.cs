using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Models.Input.Profile.Customer
{
	public class CustomerRemoveAdressInput
	{
		public string CustomerId { get; set; }
		public int adressId { get; set; }
	}
}
