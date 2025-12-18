using System.Security.Claims;

namespace Foodo.Application.Factory.Product
{
	public interface IProductStrategyFactory
	{
		IProductStrategy GetStrategy(ClaimsPrincipal user);
	}
}
