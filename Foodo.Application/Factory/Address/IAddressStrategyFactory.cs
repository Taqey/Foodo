using Foodo.Application.Commands.Addresses.CreateAddress;
using Foodo.Application.Commands.Addresses.DeleteAddress;
using System.Security.Claims;

namespace Foodo.Application.Factory.Address
{
	public interface IAddressStrategyFactory
	{
		CreateAddressCommand GetCreateAddressStrategy(ClaimsPrincipal user);
		DeleteAddressCommand GetDeleteAddressStrategy(ClaimsPrincipal user, int id);
	}
}
