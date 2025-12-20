using Foodo.Application.Abstraction.InfrastructureRelatedServices.Cache;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Enums;
using Foodo.Domain.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Application.Commands.Orders.PlaceOrder
{
	public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, ApiResponse>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICacheService _cacheService;

		public PlaceOrderCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
		{
			_unitOfWork = unitOfWork;
			_cacheService = cacheService;
		}
		public async Task<ApiResponse> Handle(PlaceOrderCommand input, CancellationToken cancellationToken)
		{
			await using var transaction = await _unitOfWork.BeginTransactionAsync();
			try
			{
				var firstItem = input.Items.First();
				var product = await _unitOfWork.ProductCustomRepository.ReadProducts().Where(p => p.ProductId == firstItem.ItemId).Select(e => new { e.Merchant.UserId }).FirstOrDefaultAsync();
				if (product == null)
					return ApiResponse.Failure("Product not found");

				var merchantId = product.UserId;
				var customerAdress = await _unitOfWork.UserCustomRepository.ReadCustomer().Where(e => e.Id == input.CustomerId).SelectMany(e => e.TblAdresses).Where(e => e.IsDefault == true).Select(e => e.AddressId).FirstOrDefaultAsync();
				if (customerAdress == 0)
					return ApiResponse.Failure("Customer has no default address");

				var order = new TblOrder
				{
					CustomerId = input.CustomerId,
					MerchantId = merchantId,
					OrderDate = DateTime.UtcNow,
					OrderStatus = OrderState.Pending,
					BillingAddressId = customerAdress
				};

				var createdOrder = await _unitOfWork.OrderRepository.CreateAsync(order);
				await _unitOfWork.saveAsync();

				var orderItems = new List<TblProductsOrder>();
				decimal totalAmount = 0;

				foreach (var item in input.Items)
				{
					var orderItem = new TblProductsOrder
					{
						OrderId = createdOrder.OrderId,
						ProductId = item.ItemId,
						Quantity = item.Quantity,
						Price = item.Price,
					};
					totalAmount += item.Price * item.Quantity;
					orderItems.Add(orderItem);
				}

				await _unitOfWork.ProductsOrderRepository.CreateRangeAsync(orderItems);
				await _unitOfWork.saveAsync();

				createdOrder.Tax = Tax.Apply(totalAmount);
				createdOrder.TotalPrice = totalAmount + createdOrder.Tax;

				_unitOfWork.OrderRepository.Update(createdOrder);
				await _unitOfWork.saveAsync();

				await _unitOfWork.CommitTransactionAsync(transaction);

				_cacheService.RemoveByPrefix($"customer_order:list:{input.CustomerId}");
				_cacheService.RemoveByPrefix($"merchant_order:list:{merchantId}");
				_cacheService.RemoveByPrefix($"merchant_customer:list:{order.MerchantId}");


				return ApiResponse.Success("Order placed successfully");
			}
			catch (Exception e)
			{
				await _unitOfWork.RollbackTransactionAsync(transaction);
				return ApiResponse.Failure($"Failed to place order: {e.Message}");
			}
		}
	}
}
