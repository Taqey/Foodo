using Foodo.Application.Models.Dto.Photo;
using System.Text.Json.Serialization;

namespace Foodo.Application.Models.Dto.Product
{
	[JsonDerivedType(typeof(MerchantProductDto))]
	[JsonDerivedType(typeof(CustomerProductDto))]

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
