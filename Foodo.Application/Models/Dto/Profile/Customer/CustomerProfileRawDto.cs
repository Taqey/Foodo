namespace Foodo.Application.Models.Dto.Profile.Customer
{
	public class CustomerProfileRawDto
	{
		public int AddressId { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string StreetAddress { get; set; }
		public string PostalCode { get; set; }
		public string Country { get; set; }
		public bool IsDefault { get; set; }

		// User
		public string Id { get; set; }
		public string PhoneNumber { get; set; }
		public bool EmailConfirmed { get; set; }
		public string Email { get; set; }

		// Customer
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Gender { get; set; }
		public DateTime BirthDate { get; set; }
	}
}
