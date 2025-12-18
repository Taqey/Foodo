using Foodo.Application.Models.Dto.Photo;

namespace Foodo.Application.Models.Dto.Product
{
	public class ProductBaseDto
	{
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public string ProductDescription { get; set; }
		public string Price { get; set; }
		public ICollection<string> ProductCategories { get; set; } = new List<string>();
		public ICollection<ProductPhotosDto> Urls { get; set; } = new List<ProductPhotosDto>();
	}
}
