using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Foodo.Application.Models.Dto.Order
{
	[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
	[JsonDerivedType(typeof(CustomerOrderDto), "customer")]
	[JsonDerivedType(typeof(MerchantOrderDto), "merchant")]
	public class OrderBaseDto
	{
		public int OrderId { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal? TotalAmount { get; set; }
		public string Status { get; set; }
		public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
	}
}
