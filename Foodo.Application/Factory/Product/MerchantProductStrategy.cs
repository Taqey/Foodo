using Foodo.Application.Abstraction.Product;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Order;
using Foodo.Application.Models.Dto.Product;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Factory.Product
{
	public class MerchantProductStrategy : IProductStrategy
	{
		private readonly IMerchantProductService _service;

		public MerchantProductStrategy(IMerchantProductService service)
		{
			_service = service;
		}
		public async Task<ApiResponse<ProductBaseDto>> ReadProduct(ItemByIdInput input)
		{
			var response = await _service.ReadProduct(input);
			if (!response.IsSuccess)
				return ApiResponse<ProductBaseDto>.Failure(response.Message);
			var result = response.Data;
			return ApiResponse<ProductBaseDto>.Success(result);
		}

		public async Task<ApiResponse<PaginationDto<ProductBaseDto>>> ReadProducts(ProductPaginationInput input)
		{
			var response = await _service.ReadProducts(input);
			if (!response.IsSuccess)
				return ApiResponse<PaginationDto<ProductBaseDto>>.Failure(response.Message);
			var mapped = new PaginationDto<ProductBaseDto>
			{
				TotalPages = response.Data.TotalPages,
				TotalItems = response.Data.TotalItems,
				Items = response.Data.Items.Cast<ProductBaseDto>().ToList()
			};

			return ApiResponse<PaginationDto<ProductBaseDto>>.Success(mapped);
		}
	}
}
