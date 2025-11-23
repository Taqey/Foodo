namespace Foodo.Application.Models.Dto
{
	public class CustomerDto
	{
		public string FullName { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public DateTime LastPurchased { get; set; }
		public int TotalOrders { get; set; }
		public decimal TotalSpent { get; set; }

	}
}