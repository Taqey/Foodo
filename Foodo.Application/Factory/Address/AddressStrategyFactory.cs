using Foodo.Application.Commands.Addresses.CreateAddress;
using Foodo.Application.Commands.Addresses.CreateAddress.CreateCustomerAddress;
using Foodo.Application.Commands.Addresses.CreateAddress.CreateMerchantAddress;
using Foodo.Application.Commands.Addresses.DeleteAddress;
using Foodo.Application.Commands.Addresses.DeleteAddress.DeleteCustomerAddress;
using Foodo.Application.Commands.Addresses.DeleteAddress.DeleteMerchantAddress;
using Foodo.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Foodo.Application.Factory.Address
{
	public class AddressStrategyFactory : IAddressStrategyFactory
	{
		public CreateAddressCommand GetCreateAddressStrategy(ClaimsPrincipal user)
		{
			var role=user.FindFirst(ClaimTypes.Role)?.Value;
			switch (role)
			{
				case nameof(UserType.Customer):
					return new CreateCustomerAddressCommand();
				case nameof(UserType.Merchant):
					return new CreateMerchantAddressCommand();
				default:
					throw new ArgumentException("Invalid role");
			}
		}

		public DeleteAddressCommand GetDeleteAddressStrategy(ClaimsPrincipal user, int id)
		{
			var role = user.FindFirst(ClaimTypes.Role)?.Value;
			switch (role)
			{
				case nameof(UserType.Customer):
					return new DeleteCustomerAddressCommand{userId=user.FindFirst(ClaimTypes.NameIdentifier)?.Value,adressId=id};
				case nameof(UserType.Merchant):
					return new DeleteMerchantAddressCommand { userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value, adressId = id };
				default:
					throw new ArgumentException("Invalid role");
			}
		}
	}
}
