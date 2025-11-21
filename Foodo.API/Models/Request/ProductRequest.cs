using Foodo.Application.Models.Dto;

namespace Foodo.Application.Abstraction.Merchant
{
	public class ProductRequest
	{
		public string ProductName { get; set; }
		public string ProductDescription { get; set; }
		public string Price { get; set; }
		public ICollection<AttributeDto>? Attributes { get; set; } = new List<AttributeDto>();
	}
}