using Foodo.Application.Models.Dto.Profile.Merchant;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Models.Dto.Profile.Customer
{
	public class CustomerProfileDto
	{
		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string Gender { get; set; }
		public DateOnly BirthDate { get; set; }
		public string Email { get; set; }
		public bool IsEmailConfirmed { get; set; }
		public string PhoneNumber { get; set; }
		public List<CustomerAdressDto>? Adresses { get; set; } = new List<CustomerAdressDto>();

	}
}
