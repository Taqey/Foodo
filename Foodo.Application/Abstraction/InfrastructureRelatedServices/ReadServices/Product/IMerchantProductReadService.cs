using Foodo.Application.Models.Dto.Product;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Product
{
	public interface IMerchantProductReadService
	{
		Task<MerchantProductDto> ReadProduct(int productId);

	}
}
