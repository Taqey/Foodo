namespace Foodo.Domain.Entities
{
	public class LkpUserPhoto
	{
		public string UserId { get; set; }
		public string Url { get; set; }
		public virtual ApplicationUser user { get; set; }

	}
}
