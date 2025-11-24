using Foodo.Domain.Enums;

namespace Foodo.Application.Models.Dto
{
	public class ProductCategoryDto
	{
		public List<string> categories { get; set; } = new List<string>();
	}
}