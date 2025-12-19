using Foodo.Application.Models.Enums;
using Foodo.Application.Queries.Products.GetProduct.GetProduct;
using Foodo.Application.Queries.Products.GetProducts.GetProducts;
using Foodo.Domain.Enums;
using System.Security.Claims;

namespace Foodo.Application.Factory.Product
{
	public interface IProductStrategyFactory
	{
		GetProductQuery GetProductStrategy(ClaimsPrincipal user, int productId);
		GetProductsQuery GetProductsStrategy(ClaimsPrincipal user, int PageNumber, int PageSize, FoodCategory? categoryId, string? restaurantId, ProductOrderBy? orderBy, OrderingDirection? orderingDirection);

	}
}
