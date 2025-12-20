namespace Foodo.Application.Models.Dto.Profile.Merchant
{
	public class MerchantProfileRawDto
	{
		// Address
		public int AddressId { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string StreetAddress { get; set; }
		public string PostalCode { get; set; }
		public string Country { get; set; }
		//public bool IsDefault { get; set; }

		// User
		public string Id { get; set; }
		public string PhoneNumber { get; set; }
		public bool EmailConfirmed { get; set; }
		public string Email { get; set; }

		// Merchant
		public string StoreName { get; set; }
		public string StoreDescription { get; set; }
		public int? CategoryId { get; set; }

	}
}
