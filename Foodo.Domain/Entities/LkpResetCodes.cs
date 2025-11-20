namespace Foodo.Domain.Entities
{
	public class LkpResetCodes
	{
		public int Id { get; set; }
		public string Key { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime ExpiresAt { get; set; }
		public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
		public bool? IsUsed { get; set; } = false;

	}
}
