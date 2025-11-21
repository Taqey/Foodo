namespace Foodo.Application.Models.Dto
{
	public class ProductDto
	{
		public int ProducId { get; set; }

		public string ProductName { get; set; }
		public string ProductDescription { get; set; }
		public string Price { get; set; }
		public ICollection<AttributeDto> Attributes { get; set; } = new List<AttributeDto>();
	}
}