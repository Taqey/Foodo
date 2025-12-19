using Foodo.Application.Commands.Addresses.CreateAddress;
using Foodo.Application.Commands.Addresses.DeleteAddress;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Foodo.Application.Factory.Address
{
	public interface IAddressStrategyFactory
	{
		CreateAddressCommand GetCreateAddressStrategy(ClaimsPrincipal user);
		DeleteAddressCommand GetDeleteAddressStrategy(ClaimsPrincipal user,int id);
	}
}
