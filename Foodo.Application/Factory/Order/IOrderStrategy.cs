using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Order;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Factory.Order
{
	public interface IOrderStrategy
	{
		Task<ApiResponse<OrderBaseDto>> GetOrder(int id);
		Task<ApiResponse<PaginationDto<OrderBaseDto>>> GetOrders(ProductPaginationInput input);
	}
}
