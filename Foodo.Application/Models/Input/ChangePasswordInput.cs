namespace Foodo.Application.Models.Input
{
	public class ChangePasswordInput
	{
		public string CurrentPassword { get; set; }
		public string NewPassword { get; set; }
		public string UserId { get; set; }
	}
}