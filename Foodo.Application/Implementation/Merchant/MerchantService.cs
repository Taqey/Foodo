using Foodo.Application.Abstraction;
using Foodo.Application.Abstraction.Merchant;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Enums;
using Foodo.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Implementation.Merchant;

public class MerchantService : IMerchantService
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IUserService _service;
	private readonly ICacheService _cacheService;

	public MerchantService(IUnitOfWork unitOfWork, IUserService service, ICacheService cacheService)
	{
		_unitOfWork = unitOfWork;
		_service = service;
		_cacheService = cacheService;
	}

	#region Products

	public async Task<ApiResponse> CreateProductAsync(ProductInput input)
	{
		var transaction = await _unitOfWork.BeginTransactionAsync();

		try
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
			var Categories = input.Categories;

			foreach (var item in Categories)
			{
				Productresult.ProductCategory.Add(new TblProductCategory { productid = Productresult.ProductId, categoryid = (int)item });
			}
			await _unitOfWork.saveAsync();

			// Clear cache
			_cacheService.RemoveByPrefix($"product:list:merchant:{input.Id}");
			await transaction.CommitAsync();
		}
		catch (Exception e)
		{
			await transaction.RollbackAsync();
			return ApiResponse.Failure($"Failed to create product: {e.Message}");
		}

		return ApiResponse.Success("Product created successfully");

	}

	public async Task<ApiResponse<PaginationDto<ProductDto>>> ReadAllProductsAsync(ProductPaginationInput input)
	{
		string cacheKey = $"product:list:merchant:{input.UserId}:{input.Page}:{input.PageSize}";

		var cached = _cacheService.Get<PaginationDto<ProductDto>>(cacheKey);
		if (cached != null)
		{
			return ApiResponse<PaginationDto<ProductDto>>.Success(cached);
		}

		var (products, totalCount, totalPages) = await _unitOfWork.ProductRepository.PaginationAsync(
			input.Page,
			input.PageSize,
			p => p.UserId == input.UserId
		);

		var productDtos = new List<ProductDto>();

		foreach (var product in products)
		{
			var productDto = new ProductDto
			{
				ProductId = product.ProductId,
				ProductName = product.ProductsName,
				ProductDescription = product.ProductDescription,
				Price = product.TblProductDetails.FirstOrDefault()?.Price.ToString() ?? "0",
				Attributes = new List<AttributeDto>(),
				ProductCategories = product.ProductCategory
				.Select(c => c.Category.CategoryName) // مباشرة string
				.ToList()
			};

			// Attributes
			var detail = product.TblProductDetails.FirstOrDefault();
			if (detail != null)
			{
				foreach (var pda in detail.LkpProductDetailsAttributes)
				{
					var attributeDto = new AttributeDto
					{
						Name = pda.Attribute.Name,
						Value = pda.Attribute.value,
						MeasurementUnit = pda.UnitOfMeasure.UnitOfMeasureName
					};

					productDto.Attributes.Add(attributeDto);
				}
			}



			productDtos.Add(productDto);
		}

		var paginationDto = new PaginationDto<ProductDto>
		{
			TotalItems = totalCount,
			TotalPages = totalPages,
			Items = productDtos
		};

		_cacheService.Set(cacheKey, paginationDto);

		return ApiResponse<PaginationDto<ProductDto>>.Success(paginationDto);
	}


	public async Task<ApiResponse<ProductDto>> ReadProductByIdAsync(int productId)
	{
		string cacheKey = $"product:{productId}";
		var cached = _cacheService.Get<ProductDto>(cacheKey);
		if (cached != null) return ApiResponse<ProductDto>.Success(cached);

		var product = await _unitOfWork.ProductRepository.ReadByIdAsync(productId);
		if (product == null) return ApiResponse<ProductDto>.Failure("Product not found");

		var productDetail = product.TblProductDetails.FirstOrDefault();

		var productDto = new ProductDto
		{
			ProductId = product.ProductId,
			ProductName = product.ProductsName,
			ProductDescription = product.ProductDescription,
			Price = productDetail?.Price.ToString() ?? "0",
			Attributes = new List<AttributeDto>(),
			ProductCategories = product.ProductCategory
				.Select(c => c.Category.CategoryName) // مباشرة string
				.ToList()
		};

		if (productDetail != null)
		{
			foreach (var pda in productDetail.LkpProductDetailsAttributes)
			{
				productDto.Attributes.Add(new AttributeDto
				{
					Name = pda.Attribute.Name,
					Value = pda.Attribute.value,
					MeasurementUnit = pda.UnitOfMeasure.UnitOfMeasureName
				});
			}
		}

		_cacheService.Set(cacheKey, productDto);
		return ApiResponse<ProductDto>.Success(productDto);
	}


	public async Task<ApiResponse> UpdateProductAsync(ProductUpdateInput input)
	{
		var product = await _unitOfWork.ProductRepository.ReadByIdAsync(input.productId);
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

		var a = await _unitOfWork.saveAsync();

		// Clear cache
		_cacheService.Remove($"product:{input.productId}");
		_cacheService.RemoveByPrefix($"product:list:merchant:{product.UserId}");

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

		// Clear cache
		_cacheService.Remove($"product:{productId}");
		_cacheService.RemoveByPrefix($"product:list:merchant:{product.UserId}");

		return ApiResponse.Success("Product deleted successfully");
	}
	
	public async Task<ApiResponse> AddProductAttributeAsync(int productId, AttributeCreateInput attributes)
	{
		var product = await _unitOfWork.ProductRepository.ReadByIdAsync(productId);
		if (product == null)
			return ApiResponse.Failure("Product not found");

		var productDetail = product.TblProductDetails.FirstOrDefault();
		if (productDetail == null)
			return ApiResponse.Failure("Product details not found");

		using var transaction = await _unitOfWork.BeginTransactionAsync();

		foreach (var item in attributes.Attributes)
		{
			var attribute = new LkpAttribute { Name = item.Name, value = item.Value };
			await _unitOfWork.AttributeRepository.CreateAsync(attribute);

			var measureUnit = new LkpMeasureUnit { UnitOfMeasureName = item.MeasurementUnit };
			await _unitOfWork.MeasureUnitRepository.CreateAsync(measureUnit);

			var productDetailAttribute = new LkpProductDetailsAttribute
			{
				Attribute = attribute,           
				UnitOfMeasure = measureUnit,     
				ProductDetailId = productDetail.ProductDetailId
			};
			await _unitOfWork.ProductDetailsAttributeRepository.CreateAsync(productDetailAttribute);
		}

		await _unitOfWork.saveAsync();
		await transaction.CommitAsync();

		_cacheService.Remove($"product:{productId}");
		_cacheService.RemoveByPrefix($"product:list:merchant:{product.UserId}");

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

		// Clear cache
		_cacheService.Remove($"product:{productId}");
		_cacheService.RemoveByPrefix($"product:list:merchant:{product.UserId}");

		return ApiResponse.Success("Attributes removed successfully");
	}

	public async Task<ApiResponse> AddProductCategoriesAsync(ProductCategoryInput categoryInput)
	{
		var product = await _unitOfWork.ProductRepository.ReadByIdAsync(categoryInput.ProductId);
		if (product == null)
		{
			return ApiResponse.Failure("Product not found");
		}
		foreach (var item in categoryInput.restaurantCategories)
		{
			if (product.ProductCategory.Where(c => c.categoryid == (int)item).Any())
			{
				continue;
			}
			product.ProductCategory.Add(new TblProductCategory { productid = categoryInput.ProductId, categoryid = (int)item });
		}
		var result = await _unitOfWork.saveAsync();

		// Clear cache
		_cacheService.Remove($"product:{product.ProductId}");
		_cacheService.RemoveByPrefix($"product:list:merchant:{product.UserId}");

		return ApiResponse.Success("Categories added successfully");
	}

	public async Task<ApiResponse> RemoveProductCategoriesAsync(ProductCategoryInput categoryInput)
	{
		var product = await _unitOfWork.ProductRepository.ReadByIdAsync(categoryInput.ProductId);
		if (product == null)
			return ApiResponse.Failure("Product not found");

		foreach (var item in categoryInput.restaurantCategories)
		{
			var category = product.ProductCategory
				.FirstOrDefault(c => c.categoryid == (int)item);

			if (category != null)
				product.ProductCategory.Remove(category);
		}

		await _unitOfWork.saveAsync();

		// Clear cache
		_cacheService.Remove($"product:{product.ProductId}");
		_cacheService.RemoveByPrefix($"product:list:merchant:{product.UserId}");

		return ApiResponse.Success("Categories removed successfully");
	}

	#endregion

	#region Orders

	public async Task<ApiResponse<PaginationDto<MerchantOrderDto>>> ReadAllOrdersAsync(ProductPaginationInput input)
	{
		string cacheKey = $"order:list:merchant:{input.UserId}:{input.Page}";

		if (_cacheService.Get<PaginationDto<MerchantOrderDto>>(cacheKey) is PaginationDto<MerchantOrderDto> cachedOrders)
			return ApiResponse<PaginationDto<MerchantOrderDto>>.Success(cachedOrders);

		var (orders, totalCount, totalPages) = await _unitOfWork.OrderRepository.PaginationAsync(
			input.Page,
			input.PageSize,
			o => o.MerchantId == input.UserId
		);

		if (orders == null || !orders.Any())
			return ApiResponse<PaginationDto<MerchantOrderDto>>.Failure("No orders found");

		var merchant = await _service.GetByIdAsync(input.UserId);
		string merchantName = merchant?.TblMerchant?.StoreName ?? "Unknown";

		var orderDtos = new List<MerchantOrderDto>();

		foreach (var order in orders)
		{
			var customer = await _service.GetByIdAsync(order.CustomerId);
			string customerName = customer?.TblCustomer?.FirstName ?? "Unknown";

			var orderDto = new MerchantOrderDto
			{
				OrderId = order.OrderId,
				CustomerId = order.CustomerId,
				CustomerName = customerName,
				OrderDate = order.OrderDate,
				TotalAmount = order.TotalPrice,
				Status = order.OrderStatus.ToString(),
				OrderItems = order.TblProductsOrders.Select(item => new OrderItemDto
				{
					ItemId = item.ProductId,
					ItemName = item.Product.ProductsName,
					Quantity = item.Quantity,
					Price = item.Price
				}).ToList()
			};

			orderDtos.Add(orderDto);
		}

		var paginationDto = new PaginationDto<MerchantOrderDto>
		{
			TotalItems = totalCount,
			TotalPages = totalPages,
			Items = orderDtos
		};

		_cacheService.Set(cacheKey, paginationDto);

		return ApiResponse<PaginationDto<MerchantOrderDto>>.Success(paginationDto);
	}

	public async Task<ApiResponse<MerchantOrderDto>> ReadOrderByIdAsync(int orderId)
	{
		string cacheKey = $"order:{orderId}";
		var cached = _cacheService.Get<MerchantOrderDto>(cacheKey);
		if (cached != null) return ApiResponse<MerchantOrderDto>.Success(cached);

		var order = await _unitOfWork.OrderRepository.ReadByIdAsync(orderId);
		if (order == null)
		{
			return ApiResponse<MerchantOrderDto>.Failure("Order not found");
		}
		var MerchantName = (await _service.GetByIdAsync(order.MerchantId))?.TblMerchant.StoreName;
		var CustomerId = (await _service.GetByIdAsync(order.CustomerId));
		var CustomerName = CustomerId?.TblCustomer.FirstName;
		var orderDto = new MerchantOrderDto
		{
			OrderId = order.OrderId,
			CustomerId = order.CustomerId,
			CustomerName = CustomerName,
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

		_cacheService.Set(cacheKey, orderDto);
		return ApiResponse<MerchantOrderDto>.Success(orderDto);

	}

	public async Task<ApiResponse> UpdateOrderStatusAsync(int orderId, OrderStatusUpdateInput input)
	{
		var order = await _unitOfWork.OrderRepository.ReadByIdAsync(orderId);
		var status = Enum.Parse<OrderState>(input.Status);
		order.OrderStatus = status;
		await _unitOfWork.saveAsync();

		// Clear cache
		_cacheService.Remove($"order:{orderId}");
		_cacheService.RemoveByPrefix($"order:list:merchant:{order.MerchantId}");

		return ApiResponse.Success("Order status updated successfully");
	}

	#endregion

	#region Customers

	public async Task<ApiResponse<PaginationDto<CustomerDto>>> ReadAllPurchasedCustomersAsync(ProductPaginationInput input)
	{
		var cacheKey = $"customer:list:merchant:{input.UserId}:{input.Page}:{input.PageSize}";

		if (_cacheService.Get<PaginationDto<CustomerDto>>(cacheKey) is PaginationDto<CustomerDto> cachedResult)
			return ApiResponse<PaginationDto<CustomerDto>>.Success(cachedResult);

		var (orders, totalCount, totalPages) = await _unitOfWork.OrderRepository.PaginationAsync(
			input.Page, input.PageSize,
			o => o.MerchantId == input.UserId
		);

		if (orders == null || !orders.Any())
			return ApiResponse<PaginationDto<CustomerDto>>.Failure("No customers found");

		var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();

		var customers = await _unitOfWork.CustomerRepository
			.FindAllByContidtionAsync(c => customerIds.Contains(c.UserId));

		var groupedOrders = orders
			.GroupBy(o => o.CustomerId)
			.ToDictionary(g => g.Key, g => g.ToList());

		var customerDtos = new List<CustomerDto>();

		foreach (var c in customers)
		{
			if (!groupedOrders.TryGetValue(c.UserId, out var custOrders))
				continue;

			var dto = new CustomerDto
			{
				FullName = $"{c.FirstName} {c.LastName}",
				Email = c.User?.Email,
				PhoneNumber = c.User?.PhoneNumber,
				LastPurchased = custOrders.Max(o => o.OrderDate),
				TotalOrders = custOrders.Count,
				TotalSpent = custOrders.Sum(o => Convert.ToDecimal(o.TotalPrice))
			};

			customerDtos.Add(dto);
		}

		var paginationDto = new PaginationDto<CustomerDto>
		{
			TotalItems = totalCount,
			TotalPages = totalPages,
			Items = customerDtos
		};

		_cacheService.Set(cacheKey, paginationDto);

		return ApiResponse<PaginationDto<CustomerDto>>.Success(paginationDto);
	}

	#endregion
}
