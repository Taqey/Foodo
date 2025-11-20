namespace Foodo.Application.Models.Dto
{
	public class JwtDto
	{
		public string Token { get; set; }
		public DateTime CreatedOn { get; set; }

		public DateTime ExpiresOn { get; set; }
	}
}
