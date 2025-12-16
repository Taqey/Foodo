using Foodo.Application.Abstraction.Orders;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Order;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Factory.Order
{
	public class CustomerOrderStrategy : IOrderStrategy
	{
		private readonly ICustomerOrderService _service;

		public CustomerOrderStrategy(ICustomerOrderService service)
		{
			_service = service;
		}
		public async Task<ApiResponse<OrderBaseDto>> GetOrder(int id)
		{
			var response = await _service.GetOrderByIdAsync(id);

			if (!response.IsSuccess)
				return ApiResponse<OrderBaseDto>.Failure(response.Message);

			OrderBaseDto dto = response.Data;

			return ApiResponse<OrderBaseDto>.Success(dto);
		}


		public async Task<ApiResponse<PaginationDto<OrderBaseDto>>> GetOrders(ProductPaginationInput input)
		{
			var response = await _service.GetOrdersAsync(input);
			if (!response.IsSuccess)
				return ApiResponse<PaginationDto<OrderBaseDto>>.Failure(response.Message);
			var mapped = new PaginationDto<OrderBaseDto>
			{
				TotalPages = response.Data.TotalPages,
				TotalItems = response.Data.TotalItems,
				Items = response.Data.Items.Cast<OrderBaseDto>().ToList()
			};

			return ApiResponse<PaginationDto<OrderBaseDto>>.Success(mapped);
		}
	}
}
