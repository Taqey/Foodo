using Foodo.Domain.Enums;

namespace Foodo.API.Models.Request.Merchant
{
	public class OrderStatusUpdateRequest
	{
		public OrderState Status { get; set; }
	}
}