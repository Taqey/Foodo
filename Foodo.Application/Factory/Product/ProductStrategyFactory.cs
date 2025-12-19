using Foodo.Application.Models.Enums;
using Foodo.Application.Queries.Products.GetProduct.GetCustomerProduct;
using Foodo.Application.Queries.Products.GetProduct.GetMerchantProduct;
using Foodo.Application.Queries.Products.GetProduct.GetProduct;
using Foodo.Application.Queries.Products.GetProducts.GetCustomerProducts;
using Foodo.Application.Queries.Products.GetProducts.GetMerchantProducts;
using Foodo.Application.Queries.Products.GetProducts.GetProducts;
using Foodo.Domain.Enums;
using System.Security.Claims;

namespace Foodo.Application.Factory.Product
{
	public class ProductStrategyFactory : IProductStrategyFactory
	{
		public GetProductsQuery GetProductsStrategy(ClaimsPrincipal user, int PageNumber, int PageSize, FoodCategory? categoryId, string? restaurantId, ProductOrderBy? orderBy, OrderingDirection? orderingDirection)
		{
			var role = user.FindFirst(ClaimTypes.Role)?.Value;

			switch (role)
			{
				case nameof(UserType.Merchant):
					var Id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
					return new GetMerchantProductsQuery { Page = PageNumber, PageSize = PageSize, restaurantId = Id, categoryId = categoryId, orderBy = orderBy, orderingDirection = orderingDirection };
				case nameof(UserType.Customer):
					return new GetCustomerProductsQuery { Page = PageNumber, PageSize = PageSize, restaurantId = restaurantId, categoryId = categoryId, orderBy = orderBy, orderingDirection = orderingDirection };

				default:
					return new GetCustomerProductsQuery { Page = PageNumber, PageSize = PageSize, restaurantId = restaurantId, categoryId = categoryId, orderBy = orderBy, orderingDirection = orderingDirection };
			}
		}

		public GetProductQuery GetProductStrategy(ClaimsPrincipal user, int productId)
		{
			var role = user.FindFirst(ClaimTypes.Role)?.Value;
			switch (role)
			{
				case nameof(UserType.Merchant):
					return new GetMerchantProductQuery { productId = productId };
				case nameof(UserType.Customer):
					return new GetCustomerProductQuery { productId = productId };

				default:
					return new GetCustomerProductQuery { productId = productId };
			}
		}
	}
}
