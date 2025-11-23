using Foodo.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Models.Dto
{
	public class OrderDto
	{
		public int OrderId { get; set; }
		public string CustomerId { get; set; }
		public string CustomerName { get; set; }
		public string MerchantId { get; set; }
		public string MerchantName { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal? TotalAmount { get; set; }
		public string Status { get; set; }
		public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
	}
}
