using Foodo.Application.Queries.Products.GetProduct.GetProduct;
using System.Security.Claims;

namespace Foodo.Application.Factory.Product
{
	public interface IProductStrategyFactory
	{
		GetProductQuery GetProductStrategy(ClaimsPrincipal user,int productId);
		//GetProductsQuery GetProductsStrategy(ClaimsPrincipal user, int productId);

	}
}
