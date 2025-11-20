namespace Foodo.Application.Models.Dto
{
	public class RefreshTokenDto
	{
		public string Token { get; set; }
		public DateTime CreatedOn { get; set; }

		public DateTime ExpiresOn { get; set; }

	}
}
