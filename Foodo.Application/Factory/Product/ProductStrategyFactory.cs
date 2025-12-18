using Foodo.Application.Abstraction.Product;
using Foodo.Application.Queries.Products.GetProduct.GetCustomerProduct;
using Foodo.Application.Queries.Products.GetProduct.GetMerchantProduct;
using Foodo.Application.Queries.Products.GetProduct.GetProduct;
using Foodo.Domain.Enums;
using System.Security.Claims;

namespace Foodo.Application.Factory.Product
{
	public class ProductStrategyFactory : IProductStrategyFactory
	{

		public GetProductQuery GetProductStrategy(ClaimsPrincipal user,int productId)
		{
			var role = user.FindFirst(ClaimTypes.Role)?.Value;
			switch (role)
			{
				case nameof(UserType.Merchant):
					return new GetMerchantProductQuery{productId=productId};
				case nameof(UserType.Customer):
					return new GetCustomerProductQuery { productId = productId };

				default:
					return new GetCustomerProductQuery{ productId = productId };
			}
		}
	}
}
