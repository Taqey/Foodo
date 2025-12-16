using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Abstraction.Orders;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Order;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Merchant;
using Foodo.Application.Models.Response;
using Foodo.Domain.Enums;
using Foodo.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Implementation.Orders
{
	public class MerchantOrderService : IMerchantOrderService
	{
		private readonly ICacheService _cacheService;
		private readonly IUnitOfWork _unitOfWork;

		public MerchantOrderService(ICacheService cacheService, IUnitOfWork unitOfWork)
		{
			_cacheService = cacheService;
			_unitOfWork = unitOfWork;
		}
		public async Task<ApiResponse<MerchantOrderDto>> GetOrderByIdAsync(int orderId)
		{
			string cacheKey = $"merchant_order:{orderId}";
			var cached = _cacheService.Get<MerchantOrderDto>(cacheKey);
			if (cached != null) return ApiResponse<MerchantOrderDto>.Success(cached);

			var order = _unitOfWork.OrderCustomRepository.ReadOrders().Where(e => e.OrderId == orderId);
			var orderDto = await order.Select(e => new MerchantOrderDto
			{

				OrderId = e.OrderId,
				OrderDate = e.OrderDate,
				TotalAmount = e.TotalPrice,
				Status = e.OrderStatus.ToString(),
				CustomerId = e.CustomerId,
				CustomerName =
				_unitOfWork.UserCustomRepository.ReadCustomer().Where(p => p.Id == e.CustomerId).Select(p => p.TblCustomer.FirstName + " " + p.TblCustomer.LastName).FirstOrDefault(),
				OrderItems = e.TblProductsOrders.Select(p => new OrderItemDto
				{
					ItemId = p.ProductId,
					ItemName = p.Product.ProductsName,
					Quantity = p.Quantity,
					Price = p.Price
				}).ToList()
			}).FirstOrDefaultAsync();
			if (orderDto == null)
			{
				return ApiResponse<MerchantOrderDto>.Failure("Order not found");
			}
			_cacheService.Set(cacheKey, orderDto);
			return ApiResponse<MerchantOrderDto>.Success(orderDto);
		}

		public async Task<ApiResponse<PaginationDto<MerchantOrderDto>>> GetOrdersAsync(ProductPaginationInput input)
		{
			string cacheKey = $"merchant_order:list:{input.UserId}:{input.Page}:{input.PageSize}";

			if (_cacheService.Get<PaginationDto<MerchantOrderDto>>(cacheKey) is PaginationDto<MerchantOrderDto> cachedOrders)
				return ApiResponse<PaginationDto<MerchantOrderDto>>.Success(cachedOrders);

			var OrderQuery = _unitOfWork.OrderCustomRepository.ReadOrdersInclude().Where(e => e.MerchantId == input.UserId);
			var totalCount = await OrderQuery.CountAsync();
			var totalPages = (int)Math.Ceiling(totalCount / (double)input.PageSize);
			var FilterOrderQuery = OrderQuery.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
			if (totalCount == 0)
			{
				var emptyResult = new PaginationDto<MerchantOrderDto>
				{
					TotalItems = 0,
					TotalPages = 0,
					Items = new List<MerchantOrderDto>()
				};
				_cacheService.Set(cacheKey, emptyResult);
				return ApiResponse<PaginationDto<MerchantOrderDto>>.Success(emptyResult);
			}

			var orders = await FilterOrderQuery.Select(e => new MerchantOrderDto
			{
				OrderId = e.OrderId,
				OrderDate = e.OrderDate,
				TotalAmount = e.TotalPrice,
				Status = e.OrderStatus.ToString(),
				CustomerId = e.CustomerId,
				CustomerName =
				_unitOfWork.UserCustomRepository.ReadCustomer().Where(p => p.Id == e.CustomerId).Select(p => p.TblCustomer.FirstName + " " + p.TblCustomer.LastName).FirstOrDefault(),
				OrderItems = e.TblProductsOrders.Select(p => new OrderItemDto
				{
					ItemId = p.ProductId,
					ItemName = p.Product.ProductsName,
					Quantity = p.Quantity,
					Price = p.Price
				}).ToList()
			}).ToListAsync();


			var paginationDto = new PaginationDto<MerchantOrderDto>
			{
				TotalItems = totalCount,
				TotalPages = totalPages,
				Items = orders
			};

			_cacheService.Set(cacheKey, paginationDto);

			return ApiResponse<PaginationDto<MerchantOrderDto>>.Success(paginationDto);
		}

		public async Task<ApiResponse> UpdateOrderStatusAsync(int orderId, OrderStatusUpdateInput input)
		{
			var order = await _unitOfWork.OrderCustomRepository.ReadOrders().Where(e => e.OrderId == orderId).FirstOrDefaultAsync();
			var status = Enum.Parse<OrderState>(input.Status);
			order.OrderStatus = status;
			_unitOfWork.OrderRepository.Update(order);
			await _unitOfWork.saveAsync();

			// Clear cache
			_cacheService.Remove($"merchant_order:{orderId}");
			_cacheService.RemoveByPrefix($"merchant_order:list:{order.MerchantId}");

			_cacheService.Remove($"customer_order:{orderId}");
			_cacheService.RemoveByPrefix($"customer_order:list:{order.CustomerId}");
			_cacheService.RemoveByPrefix($"merchant_customer:list:{order.MerchantId}");



			return ApiResponse.Success("Order status updated successfully");
		}
	}
}
