using System.ComponentModel.DataAnnotations;

namespace Foodo.API.Models.Request.Authentication
{
	public class VerifyEmailRequest
	{
		[Required]
		public string Code { get; set; }
	}
}
