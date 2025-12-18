namespace Foodo.Application.Models.Dto.Product
{
	public class CustomerProductDto : ProductBaseDto
	{
		public List<AttributeDto> Attributes { get; set; } = new();

	}
}