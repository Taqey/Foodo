namespace Foodo.API.Models.Request
{
	public class ResetPasswordRequest
	{
		public string Code { get; set; }
		public string NewPassword { get; set; }
	}
}
