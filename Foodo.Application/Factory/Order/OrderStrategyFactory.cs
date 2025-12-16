using Foodo.Application.Abstraction.Orders;
using Foodo.Domain.Enums;
using System.Security.Claims;

namespace Foodo.Application.Factory.Order
{
	public class OrderStrategyFactory : IOrderStrategyFactory
	{
		private readonly ICustomerOrderService _customerService;
		private readonly IMerchantOrderService _merchantService;

		public OrderStrategyFactory(ICustomerOrderService customerService,IMerchantOrderService merchantService)
		{
			_customerService = customerService;
			_merchantService = merchantService;
		}
		public IOrderStrategy GetStrategy(ClaimsPrincipal user)
		{
			var role = user.FindFirst(ClaimTypes.Role)?.Value;
			switch (role)
			{
				case nameof(UserType.Merchant):
					return new MerchantOrderStrategy(_merchantService);
				case nameof(UserType.Customer):
					return new CustomerOrderStrategy(_customerService);

				default:
					throw new ArgumentException("Invalid role");
			}
		}
	}
}
