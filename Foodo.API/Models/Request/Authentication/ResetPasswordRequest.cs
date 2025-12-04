using System.ComponentModel.DataAnnotations;

namespace Foodo.API.Models.Request.Authentication
{
	public class ResetPasswordRequest
	{
		[Required]
		public string Code { get; set; }

		[DataType(DataType.Password)]
		[Required]
		[RegularExpression(@"^(?=.*\d)(?=.*[A-Z])(?=.*[a-z])(?=.*[^\w\d\s:])([^\s]){8,16}$",
		ErrorMessage = "Password must be 8-16 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character, and cannot contain spaces.")]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		[Required]
		[Compare("NewPassword", ErrorMessage = "Confirm Password must match New Password.")]
		public string ConfirmPassword { get; set; }
	}
}
