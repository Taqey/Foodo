using Foodo.Application.Abstraction.InfrastructureRelatedServices.Cache;
using Foodo.Application.Models.Response;
using Foodo.Domain.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Application.Commands.Orders.UpdateOrder
{
	public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, ApiResponse>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICacheService _cacheService;

		public UpdateOrderStatusCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
		{
			_unitOfWork = unitOfWork;
			_cacheService = cacheService;
		}
		public async Task<ApiResponse> Handle(UpdateOrderStatusCommand input, CancellationToken cancellationToken)
		{
			var order = await _unitOfWork.OrderCustomRepository.ReadOrders().Where(e => e.OrderId == input.OrderId).FirstOrDefaultAsync();
			order.OrderStatus = input.OrderState;
			_unitOfWork.OrderRepository.Update(order);
			await _unitOfWork.saveAsync();

			_cacheService.Remove($"merchant_order:{input.OrderId}");
			_cacheService.RemoveByPrefix($"merchant_order:list:{order.MerchantId}");
			_cacheService.Remove($"customer_order:{input.OrderId}");
			_cacheService.RemoveByPrefix($"customer_order:list:{order.CustomerId}");
			_cacheService.RemoveByPrefix($"merchant_customer:list:{order.MerchantId}");
			return ApiResponse.Success("Order status updated successfully");
		}
	}
}
