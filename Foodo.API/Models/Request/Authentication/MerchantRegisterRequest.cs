using Foodo.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Foodo.API.Models.Request.Authentication
{
	public class MerchantRegisterRequest
	{
		[Required(ErrorMessage = "Email is required")]
		[RegularExpression(
			@"^((?!\.)[\w-_.]*[^.])(@\w+)(\.\w+(\.\w+)?[^.\W])$",
			ErrorMessage = "Invalid email format"
		)]
		public string Email { get; set; }
		[DataType(DataType.Password)]
		[Required]
		[RegularExpression(@"^(?=.*\d)(?=.*[A-Z])(?=.*[a-z])(?=.*[^\w\d\s:])([^\s]){8,16}$",
	ErrorMessage = "Password must be 8-16 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character, and cannot contain spaces.")]
		public string Password { get; set; }
		[return: StringLength(30, MinimumLength = 3)]
		[Required]

		public string UserName { get; set; }
		[StringLength(50)]
		[Required]
		public string StoreName { get; set; }
		[StringLength(100)]
		[Required]
		public string StoreDescription { get; set; }


	}
}