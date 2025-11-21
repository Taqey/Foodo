using Foodo.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Foodo.API.Models.Request.Authentication
{
	public class CustomerRegisterRequest
	{
		[DataType(DataType.EmailAddress)]
		[Required]
		public string Email { get; set; }
		[DataType(DataType.Password)] 
		[Required]
		[RegularExpression(@"^(?=.*\d)(?=.*[A-Z])(?=.*[a-z])(?=.*[^\w\d\s:])([^\s]){8,16}$",
	ErrorMessage = "Password must be 8-16 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character, and cannot contain spaces.")]
		public string Password { get; set; }
		[Required]
		[StringLength(30, MinimumLength = 3)]
		public string UserName { get; set; }
		[StringLength(50)]
		[Required]

		public string FirstName { get; set; }
		[StringLength(50)]
		[Required]

		public string LastName { get; set; }
		[DataType(DataType.PhoneNumber)]
		[Required]

		public string PhoneNumber { get; set; }

		[EnumDataType(typeof(Gender))]
		[Required]

		public Gender Gender { get; set; }
		[DataType(DataType.Date)]
		[Required]

		public DateOnly DateOfBirth { get; set; }




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
