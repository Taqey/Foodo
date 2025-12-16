using Foodo.Application.Abstraction.Product;
using Foodo.Domain.Enums;
using System.Security.Claims;

namespace Foodo.Application.Factory.Product
{
	public class ProductStrategyFactory : IProductStrategyFactory
	{
		private readonly ICustomerProductService _customerProductService;
		private readonly IMerchantProductService _merchantProductService;

		public ProductStrategyFactory(ICustomerProductService customerProductService, IMerchantProductService merchantProductService)
		{
			_customerProductService = customerProductService;
			_merchantProductService = merchantProductService;
		}
		public IProductStrategy GetStrategy(ClaimsPrincipal user)
		{
			var role = user.FindFirst(ClaimTypes.Role)?.Value;
			switch (role)
			{
				case nameof(UserType.Merchant):
					return new MerchantProductStrategy(_merchantProductService);
				case nameof(UserType.Customer):
					return new CustomerProductStrategy(_customerProductService);

				default:
					return new CustomerProductStrategy(_customerProductService);
			}
		}
	}
}
