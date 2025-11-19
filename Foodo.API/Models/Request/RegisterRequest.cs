using Foodo.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Foodo.API.Models.Request
{
	public class RegisterRequest
	{
		[DataType(DataType.EmailAddress)]
		[Required]
		public string Email { get; set; }
		[DataType(DataType.Password)] 
		[Required]
		[RegularExpression(@"^(?=.*\d)(?=.*[A-Z])(?=.*[a-z])(?=.*[^\w\d\s:])([^\s]){8,16}$",
	ErrorMessage = "Password must be 8-16 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character, and cannot contain spaces.")]
		public string Password { get; set; }
		[StringLength(50)]
		public string? FirstName { get; set; }
		[StringLength(50)]
		public string? LastName { get; set; }
		[DataType(DataType.PhoneNumber)]
		public string? PhoneNumber { get; set; }

		[EnumDataType(typeof(Gender))]

		public Gender? Gender { get; set; }
		[Required]
		[StringLength(30, MinimumLength = 3)]
		public string UserName { get; set; }
		[Required]
		[EnumDataType(typeof(UserType))]
		public UserType UserType { get; set; } = UserType.Customer;
		[StringLength(50)]

		public string? StoreName { get; set; }
		[StringLength(100)]

		public string? StoreDescription { get; set; }
		[StringLength(50)]
		public string? City { get; set; }
		[StringLength(50)]

		public string? State { get; set; }
		[StringLength(100)]

		public string? StreetAddress { get; set; }
		[DataType(DataType.PostalCode)]
		[RegularExpression(@"^\d{4,10}$", ErrorMessage = "Invalid postal code")]

		public string? PostalCode { get; set; }
		[StringLength(50)]

		public string? Country { get; set; }

	}
}
