using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Models.Dto
{
	public class OrderItemDto
	{
		public int ItemId { get; set; }
		public int Quantity { get; set; }
		public decimal Price { get; set; }
	}
}
