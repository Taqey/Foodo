using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Abstraction.Orders;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Order;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Enums;
using Foodo.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Implementation.Orders
{
	public class CustomerOrderService : ICustomerOrderService
	{
		private readonly ICacheService _cacheService;
		private readonly IUnitOfWork _unitOfWork;

		public CustomerOrderService(ICacheService cacheService,IUnitOfWork unitOfWork)
		{
			_cacheService = cacheService;
			_unitOfWork = unitOfWork;
		}

		public async Task<ApiResponse<CustomerOrderDto>> GetOrderByIdAsync(int id)
		{
			string cacheKey = $"customer_order:{id}";

			var cached = _cacheService.Get<CustomerOrderDto>(cacheKey);
			if (cached != null)
				return ApiResponse<CustomerOrderDto>.Success(cached);

			int orderId = Convert.ToInt32(id);

			var orderQuery =
				_unitOfWork.OrderCustomRepository.ReadOrdersInclude()
				.Where(o => o.OrderId == orderId)
				.Select(e => new
				{
					e.OrderId,
					e.MerchantId,
					MerchantName = e.TblProductsOrders
									.Select(p => p.Product.Merchant.StoreName)
									.FirstOrDefault(),
					e.OrderDate,
					e.TotalPrice,
					Status = e.OrderStatus.ToString(),
					e.BillingAddressId,
					CustomerId = e.CustomerId,
					OrderItems = e.TblProductsOrders.Select(p => new OrderItemDto
					{
						ItemId = p.ProductId,
						ItemName = p.Product.ProductsName,
						Quantity = p.Quantity,
						Price = p.Price
					}).ToList()
				});

			var orderData = await orderQuery.FirstOrDefaultAsync();

			if (orderData == null)
				return ApiResponse<CustomerOrderDto>.Failure("Order not found");

			// --------- Retrieve Customer Addresses ----------


			string billingAddress = (await _unitOfWork.AdressRepository.ReadByIdAsync(orderId)).StreetAddress;

			// --------- Build Final DTO ----------
			var orderDto = new CustomerOrderDto
			{
				OrderId = orderData.OrderId,
				MerchantId = orderData.MerchantId,
				MerchantName = orderData.MerchantName,
				OrderDate = orderData.OrderDate,
				TotalAmount = orderData.TotalPrice,
				Status = orderData.Status,
				BillingAddress = billingAddress,
				OrderItems = orderData.OrderItems
			};

			_cacheService.Set(cacheKey, orderDto);

			return ApiResponse<CustomerOrderDto>.Success(orderDto);
		}
		public async Task<ApiResponse<PaginationDto<CustomerOrderDto>>> GetOrdersAsync(ProductPaginationInput input)
		{
			string cacheKey = $"customer_order:list:{input.UserId}:{input.Page}:{input.PageSize}";
			var cached = _cacheService.Get<PaginationDto<CustomerOrderDto>>(cacheKey);
			if (cached != null)
				return ApiResponse<PaginationDto<CustomerOrderDto>>.Success(cached);

			var OrderQuery = _unitOfWork.OrderCustomRepository.ReadOrdersInclude().Where(e => e.CustomerId == input.UserId);
			var totalCount = await OrderQuery.CountAsync();
			var totalPages = (int)Math.Ceiling(totalCount / (double)input.PageSize);
			var FilterOrderQuery = OrderQuery.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
			if (totalCount == 0)
			{
				var emptyResult = new PaginationDto<CustomerOrderDto>
				{
					TotalItems = 0,
					TotalPages = 0,
					Items = new List<CustomerOrderDto>()
				};
				_cacheService.Set(cacheKey, emptyResult);
				return ApiResponse<PaginationDto<CustomerOrderDto>>.Success(emptyResult);
			}
			var customerAddresses = await _unitOfWork.UserCustomRepository.ReadCustomer()
											.Where(c => c.Id == input.UserId)
											.SelectMany(c => c.TblAdresses)
											.ToListAsync();
			var orders = await FilterOrderQuery.AsAsyncEnumerable().Select(e => new CustomerOrderDto
			{
				OrderId = e.OrderId,
				MerchantId = e.MerchantId,
				MerchantName = e.TblProductsOrders.Select(p => p.Product.Merchant.StoreName).FirstOrDefault(),
				OrderDate = e.OrderDate,
				TotalAmount = e.TotalPrice,
				Status = e.OrderStatus.ToString(),
				BillingAddress = customerAddresses
						.Where(a => a.AddressId == e.BillingAddressId)
						.Select(a => a.StreetAddress)
						.FirstOrDefault(),
				OrderItems = e.TblProductsOrders.Select(p => new OrderItemDto
				{
					ItemId = p.ProductId,
					ItemName = p.Product.ProductsName,
					Quantity = p.Quantity,
					Price = p.Price
				}).ToList()
			}).ToListAsync();



			var resultDto = new PaginationDto<CustomerOrderDto>
			{
				TotalItems = totalCount,
				TotalPages = totalPages,
				Items = orders
			};

			_cacheService.Set(cacheKey, resultDto);
			return ApiResponse<PaginationDto<CustomerOrderDto>>.Success(resultDto);
		}
		public async Task<ApiResponse> PlaceOrder(CreateOrderInput input)
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

				// Clear customer cache
				_cacheService.RemoveByPrefix($"customer_order:list:{input.CustomerId}");
				//_cacheService.RemoveByPrefix($"customer_order:");
				// Clear merchant cache
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
		public async Task<ApiResponse> CancelOrder(ItemByIdInput input)
		{
			var order = await _unitOfWork.OrderRepository.FindByContidtionAsync(o => o.OrderId == Convert.ToInt32(input.ItemId));
			if (order == null)
			{
				return ApiResponse.Failure("Order not found");
			}

			order.OrderStatus = OrderState.Cancelled;
			var result = await _unitOfWork.saveAsync();

			if (result <= 0)
				return ApiResponse.Failure("Failed to cancel order");

			// Clear cache for this order

			_cacheService.Remove($"customer_order:{input.ItemId}");
			_cacheService.RemoveByPrefix($"customer_order:list:{order.CustomerId}");

			// Clear merchant caches
			_cacheService.RemoveByPrefix($"merchant_order:{input.ItemId}");
			_cacheService.RemoveByPrefix($"merchant_order:list:{order.MerchantId}");
			_cacheService.RemoveByPrefix($"merchant_customer:list:{order.MerchantId}");



			return ApiResponse.Success("Order cancelled successfully");
		}
	}
}
