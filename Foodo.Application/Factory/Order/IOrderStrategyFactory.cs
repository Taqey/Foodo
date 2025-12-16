using System.Security.Claims;

namespace Foodo.Application.Factory.Order
{
	public interface IOrderStrategyFactory
	{
		IOrderStrategy GetStrategy(ClaimsPrincipal user);
	}

}