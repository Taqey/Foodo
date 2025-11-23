using Foodo.Application.Abstraction;
using Foodo.Application.Abstraction.Merchant;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Enums;
using Foodo.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Implementation.Merchant;

public class ProductService : IProductService
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMemoryCache _cache;
	private readonly IUserService _service;

	public ProductService(IUnitOfWork unitOfWork, IMemoryCache cache,IUserService service)
	{
		_unitOfWork = unitOfWork;
		_cache = cache;
		_service = service;
	}
	public async Task<ApiResponse> CreateProductAsync(ProductInput input)
	{
		var Productresult = await _unitOfWork.ProductRepository.CreateAsync(new TblProduct
		{
			UserId = input.Id,
			ProductsName = input.ProductName,
			ProductDescription = input.ProductDescription,
		});
		await _unitOfWork.saveAsync();

		var ProductDetailResult = await _unitOfWork.ProductDetailRepository.CreateAsync(new TblProductDetail
		{
			ProductId = Productresult.ProductId,
			Price = Convert.ToDecimal(input.Price),
		});
		await _unitOfWork.saveAsync();

		List<LkpAttribute> attributes = new List<LkpAttribute>();
		List<LkpMeasureUnit> measureUnits = new List<LkpMeasureUnit>();
		foreach (var item in input.Attributes)
		{
			attributes.Add(await _unitOfWork.AttributeRepository.CreateAsync(new LkpAttribute { Name = item.Name, value = item.Value }));
			await _unitOfWork.saveAsync();

			measureUnits.Add(await _unitOfWork.MeasureUnitRepository.CreateAsync(new LkpMeasureUnit { UnitOfMeasureName = item.MeasurementUnit }));
			await _unitOfWork.saveAsync();

		}
		foreach (var item in attributes.Zip(measureUnits, (a, b) => new { a, b }))
		{

			await _unitOfWork.ProductDetailsAttributeRepository.CreateAsync(new LkpProductDetailsAttribute { AttributeId = item.a.AttributeId, UnitOfMeasureId = item.b.UnitOfMeasureId, ProductDetailId = ProductDetailResult.ProductDetailId });
		}
		await _unitOfWork.saveAsync();
		_cache.Remove($"products_{input.Id}");

		return ApiResponse.Success("Product created successfully");

	}

	public async Task<ApiResponse<List<ProductDto>>> ReadAllProductsAsync(string UserId)
	{
		string cacheKey = $"products_{UserId}";
		if ( _cache.TryGetValue(cacheKey, out List<ProductDto> cachedProducts))
		{
			return ApiResponse<List<ProductDto>>.Success(cachedProducts);
		}
		else
		{

			var products = await _unitOfWork.ProductRepository.FindAllByContidtionAsync(p => p.UserId == UserId);
			var productDtos = new List<ProductDto>();
			foreach (var product in products)
			{
				var productDto = new ProductDto
				{
					ProductId = product.ProductId,
					ProductName = product.ProductsName,
					ProductDescription = product.ProductDescription,
					Price = product.TblProductDetails.FirstOrDefault()?.Price.ToString() ?? "0",
					Attributes = new List<AttributeDto>()
				};
				var productDetail = product.TblProductDetails.FirstOrDefault();
				if (productDetail != null)
				{
					foreach (var pda in productDetail.LkpProductDetailsAttributes)
					{
						var attributeDto = new AttributeDto
						{
							ProductDetailAttributeId = pda.ProductDetailAttributeId,
							Name = pda.Attribute.Name,
							Value = pda.Attribute.value,
							MeasurementUnit = pda.UnitOfMeasure.UnitOfMeasureName
						};
						productDto.Attributes.Add(attributeDto);
					}
				}
				productDtos.Add(productDto);
			}
			var cacheEntryOptions = new MemoryCacheEntryOptions()
				.SetSlidingExpiration(TimeSpan.FromMinutes(30))
				.SetAbsoluteExpiration(TimeSpan.FromHours(2));
			_cache.Set(cacheKey, productDtos, cacheEntryOptions);
			return ApiResponse<List<ProductDto>>.Success(productDtos);
		}
	}

	public async Task<ApiResponse<ProductDto>> ReadProductByIdAsync(int productId)
	{
		var product = await _unitOfWork.ProductRepository.ReadByIdAsync(productId);
		if (product == null)
		{
			return ApiResponse<ProductDto>.Failure("Product not found");
		}

		var productDto = new ProductDto
		{
			ProductId = product.ProductId,
			ProductName = product.ProductsName,
			ProductDescription = product.ProductDescription,
			Price = product.TblProductDetails.FirstOrDefault()?.Price.ToString() ?? "0",
			Attributes = new List<AttributeDto>()
		};

		var productDetail = product.TblProductDetails.FirstOrDefault();
		if (productDetail != null)
		{
			foreach (var pda in productDetail.LkpProductDetailsAttributes)
			{
				var attributeDto = new AttributeDto
				{
					ProductDetailAttributeId = pda.ProductDetailAttributeId,
					Name = pda.Attribute.Name,
					Value = pda.Attribute.value,
					MeasurementUnit = pda.UnitOfMeasure.UnitOfMeasureName
				};
				productDto.Attributes.Add(attributeDto);
			}
		}

		return ApiResponse<ProductDto>.Success(productDto);
	}


	public async Task<ApiResponse> UpdateProductAsync(int productId, ProductInput input)
	{
		var product = await _unitOfWork.ProductRepository.ReadByIdAsync(productId);
		if (product == null)
		{
			return ApiResponse.Failure("Product not found");
		}

		product.ProductsName = input.ProductName;
		product.ProductDescription = input.ProductDescription;
		var detail = product.TblProductDetails.FirstOrDefault();
		if (detail != null)
		{
			detail.Price = Convert.ToDecimal(input.Price);
		}


		//_unitOfWork.ProductRepository.Update(product);

		var a = await _unitOfWork.saveAsync();
		_cache.Remove($"products_{input.Id}");

		return ApiResponse.Success("Product updated successfully");
	}
	public async Task<ApiResponse> DeleteProductAsync(int productId)
	{
		var product = await _unitOfWork.ProductRepository.ReadByIdAsync(productId);
		if (product == null)
		{
			return ApiResponse.Failure("Product not found");
		}

		_unitOfWork.ProductRepository.Delete(product);
		await _unitOfWork.saveAsync();
		_cache.Remove($"products_{product.UserId}");

		return ApiResponse.Success("Product deleted successfully");
	}

	public async Task<ApiResponse> AddProductAttributeAsync(int productId, AttributeCreateInput attributes)
	{
		var product = await _unitOfWork.ProductRepository.ReadByIdAsync(productId);
		var prodctDetail = product.TblProductDetails.FirstOrDefault();
		foreach (var item in attributes.Attributes)
		{
			var attribute = await _unitOfWork.AttributeRepository.CreateAsync(new LkpAttribute { Name = item.Name, value = item.Value });
			await _unitOfWork.saveAsync();
			var measureUnit = await _unitOfWork.MeasureUnitRepository.CreateAsync(new LkpMeasureUnit { UnitOfMeasureName = item.MeasurementUnit });
			await _unitOfWork.saveAsync();
			await _unitOfWork.ProductDetailsAttributeRepository.CreateAsync(new LkpProductDetailsAttribute { AttributeId = attribute.AttributeId, UnitOfMeasureId = measureUnit.UnitOfMeasureId, ProductDetailId = prodctDetail.ProductDetailId });
			await _unitOfWork.saveAsync();
		}
		_cache.Remove($"products_{product.UserId}");

		return ApiResponse.Success("Attributes added successfully");
	}

	public async Task<ApiResponse> RemoveProductAttributeAsync(int productId, AttributeDeleteInput attributes)
	{
		var product = await _unitOfWork.ProductRepository.ReadByIdAsync(productId);
		var productDetail = product.TblProductDetails.FirstOrDefault();
		foreach (var item in attributes.Attributes)
		{
			_unitOfWork.ProductDetailsAttributeRepository.DeleteRange(productDetail.LkpProductDetailsAttributes.Where(p => p.ProductDetailAttributeId == item));
		}

		await _unitOfWork.saveAsync();
		_cache.Remove($"products_{product.UserId}");
		return ApiResponse.Success("Attributes removed successfully");
	}

	public async Task<ApiResponse<List<OrderDto>>> ReadAllOrdersAsync(PaginationInput input)
	{
		var orders = await _unitOfWork.OrderRepository.FindAllByContidtionAsync(o => o.MerchantId == input.FilterBy);
		var MerchantName = (await _service.GetByIdAsync(input.FilterBy))?.UserName;
		var orderDtos = new List<OrderDto>();

		foreach (var order in orders)
		{
			var orderDto = new OrderDto
			{
				OrderId = order.OrderId,
				CustomerId = order.CustomerId,
				MerchantId = order.MerchantId,
				MerchantName = MerchantName,
				OrderDate = order.OrderDate,
				TotalAmount = order.TotalPrice,
				Status = order.OrderStatus.ToString(),
				OrderItems= new List<OrderItemDto>()
			

			};
			foreach (var item in order.TblProductsOrders)
			{
				var orderItemDto = new OrderItemDto
				{
					ItemId = item.ProductId,
					ItemName = item.Product.ProductsName,
					Quantity = item.Quantity,
					Price = item.Price
				};
				orderDto.OrderItems.Add(orderItemDto);
			}
			orderDtos.Add(orderDto);
		}
		return ApiResponse<List<OrderDto>>.Success(orderDtos);
	}

	public async Task<ApiResponse<OrderDto>> ReadOrderByIdAsync(int orderId)
	{
		var order =await _unitOfWork.OrderRepository.ReadByIdAsync(orderId);
		var MerchantName =(await _service.GetByIdAsync(order.MerchantId))?.UserName;
		var orderDto = new OrderDto
		{
			OrderId = order.OrderId,
			CustomerId = order.CustomerId,
			MerchantId = order.MerchantId,
			MerchantName = MerchantName,
			OrderDate = order.OrderDate,
			TotalAmount = order.TotalPrice,
			Status = order.OrderStatus.ToString(),
			OrderItems = new List<OrderItemDto>()
		};
		foreach (var item in order.TblProductsOrders)
		{
			var orderItemDto = new OrderItemDto
			{
				ItemId = item.ProductId,
				ItemName = item.Product.ProductsName,
				Quantity = item.Quantity,
				Price = item.Price
			};
			orderDto.OrderItems.Add(orderItemDto);
		}

		return ApiResponse<OrderDto>.Success(orderDto);

	}

	public async Task<ApiResponse> UpdateOrderStatusAsync(int orderId, OrderStatusUpdateInput input)
	{
		var order =await  _unitOfWork.OrderRepository.ReadByIdAsync(orderId);
		var status = Enum.Parse<OrderState>(input.Status);
		order.OrderStatus = status;
		await _unitOfWork.saveAsync();
		return ApiResponse.Success("Order status updated successfully");
	}
}
