using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Foodo.Application.Factory.Product
{
	public interface IProductStrategyFactory
	{
		IProductStrategy GetStrategy(ClaimsPrincipal user);
	}
}
