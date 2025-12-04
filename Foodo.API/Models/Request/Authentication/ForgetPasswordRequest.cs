using System.ComponentModel.DataAnnotations;

namespace Foodo.API.Models.Request.Authentication
{
	public class ForgetPasswordRequest
	{
		[Required(ErrorMessage = "Email is required")]
		[RegularExpression(
			@"^((?!\.)[\w-_.]*[^.])(@\w+)(\.\w+(\.\w+)?[^.\W])$",
			ErrorMessage = "Invalid email format"
		)]
		public string Email { get; set; }
	}
}
