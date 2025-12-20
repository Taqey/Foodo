using Foodo.Application.Abstraction.InfrastructureRelatedServices.Cache;
using Foodo.Application.Models.Response;
using Foodo.Domain.Enums;
using Foodo.Domain.Repository;
using MediatR;

namespace Foodo.Application.Commands.Orders.CancelOrder
{
	public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, ApiResponse>
	{
		private readonly ICacheService _cacheService;
		private readonly IUnitOfWork _unitOfWork;

		public CancelOrderCommandHandler(ICacheService cacheService, IUnitOfWork unitOfWork)
		{
			_cacheService = cacheService;
			_unitOfWork = unitOfWork;
		}
		public async Task<ApiResponse> Handle(CancelOrderCommand input, CancellationToken cancellationToken)
		{
			var order = await _unitOfWork.OrderRepository.FindByContidtionAsync(o => o.OrderId == Convert.ToInt32(input.OrderId));
			if (order == null)
			{
				return ApiResponse.Failure("Order not found");
			}
			order.OrderStatus = OrderState.Cancelled;
			var result = await _unitOfWork.saveAsync();

			if (result <= 0)
				return ApiResponse.Failure("Failed to cancel order");

			_cacheService.Remove($"customer_order:{input.OrderId}");
			_cacheService.RemoveByPrefix($"customer_order:list:{order.CustomerId}");
			_cacheService.RemoveByPrefix($"merchant_order:{input.OrderId}");
			_cacheService.RemoveByPrefix($"merchant_order:list:{order.MerchantId}");
			_cacheService.RemoveByPrefix($"merchant_customer:list:{order.MerchantId}");
			return ApiResponse.Success("Order cancelled successfully");
		}
	}
}
