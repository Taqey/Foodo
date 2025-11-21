using Foodo.Application.Models.Dto;

namespace Foodo.Application.Models.Input;

public class ProductInput
{
	public string Id { get; set; }
	public string ProductName { get; set; }
	public string ProductDescription { get; set; }
	public string Price { get; set; }
	public ICollection<AttributeDto>? Attributes { get; set; } = new List<AttributeDto>();
}