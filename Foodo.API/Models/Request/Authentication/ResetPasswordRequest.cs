namespace Foodo.API.Models.Request.Authentication
{
	public class ResetPasswordRequest
	{
		public string Code { get; set; }
		public string NewPassword { get; set; }
	}
}
