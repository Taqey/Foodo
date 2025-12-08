using System.ComponentModel.DataAnnotations;

namespace Foodo.Domain.Entities
{
	public class lkpRefreshToken
	{
		public int Id { get; set; }
		[StringLength(100)]
		public string Token { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime ExpiresAt { get; set; }
		public DateTime? RevokedOn { get; set; }

		public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
		public bool IsActive => !IsExpired && RevokedOn == null;



	}
}
