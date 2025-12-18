using Foodo.Application.Models.Dto.Merchant;

namespace Foodo.Application.Models.Dto.Product
{
	public class MerchantProductDto : ProductBaseDto
	{
		public List<ProductDetailAttributeDto> ProductDetailAttributes { get; set; } = new();
	}

}