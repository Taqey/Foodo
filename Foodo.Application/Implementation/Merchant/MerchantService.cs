using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Abstraction.Merchant;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Application.Models.Dto.Merchant;
using Foodo.Application.Models.Dto.Photo;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Merchant;
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

	public async Task<ApiResponse<CreateProductDto>> CreateProductAsync(ProductInput input)
	{
		var transaction = await _unitOfWork.BeginTransactionAsync();
		TblProduct Productresult;
		try
		{
			Productresult = await _unitOfWork.ProductRepository.CreateAsync(new TblProduct
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
				Productresult.ProductCategories.Add(new TblProductCategory { productid = Productresult.ProductId, categoryid = (int)item });
			}
			await _unitOfWork.saveAsync();

			// Clear cache
			_cacheService.RemoveByPrefix($"merchant_product:list:{input.Id}");
			_cacheService.RemoveByPrefix($"customer_product:list:all");
			_cacheService.RemoveByPrefix($"customer_product:list:shop:{input.Id}");
			_cacheService.RemoveByPrefix($"customer_product:list:category");



			await transaction.CommitAsync();
		}
		catch (Exception e)
		{
			await transaction.RollbackAsync();
			return ApiResponse<CreateProductDto>.Failure($"Failed to create product: {e.Message}");
		}

		return new ApiResponse<CreateProductDto> { IsSuccess = true, Message = "Product created successfully", Data = new CreateProductDto { ProductId = Productresult.ProductId } };

	}

	public async Task<ApiResponse<PaginationDto<MerchantProductDto>>> ReadAllProductsAsync(ProductPaginationInput input)
	{
		string cacheKey = $"merchant_product:list:{input.UserId}:{input.Page}:{input.PageSize}";

		var cached = _cacheService.Get<PaginationDto<MerchantProductDto>>(cacheKey);
		if (cached != null)
		{
			return ApiResponse<PaginationDto<MerchantProductDto>>.Success(cached);
		}
		var products = _unitOfWork.ProductCustomRepository.ReadProducts().Where(p => p.UserId == input.UserId).Skip((input.Page-1)*input.PageSize).Take(input.PageSize);
		var totalCount=await products.CountAsync();
		if (totalCount==0)
		{
			return new ApiResponse<PaginationDto<MerchantProductDto>> { IsSuccess = false, Message = "No products found" };
		}
		var totalPages = (int)Math.Ceiling((decimal)totalCount / input.PageSize);
		var productDtos =await products.Select(e=>new MerchantProductDto
		{
			ProductId=e.ProductId,
			ProductName=e.ProductsName,
			ProductDescription=e.ProductDescription,
			Price = e.TblProductDetails.Select(p => p.Price).FirstOrDefault().ToString(),
			ProductCategories = e.ProductCategories.Select(p=>p.Category.CategoryName).ToList(),
			ProductDetailAttributes=e.TblProductDetails.SelectMany(p=>p.LkpProductDetailsAttributes).Select(p=>new ProductDetailAttributeDto
			{
			Id=p.ProductDetailAttributeId,
			AttributeName=p.Attribute.Name,
			AttributeValue=p.Attribute.value,
			MeasurementUnit=p.UnitOfMeasure.UnitOfMeasureName
			}).ToList(),
			Urls=e.ProductPhotos.Select(p=>new ProductPhotosDto { isMain=p.isMain,url=p.Url}).ToList()

		}).ToListAsync();

		var paginationDto = new PaginationDto<MerchantProductDto>
		{
			TotalItems = totalCount,
			TotalPages = totalPages,
			Items = productDtos
		};

		_cacheService.Set(cacheKey, paginationDto);

		return ApiResponse<PaginationDto<MerchantProductDto>>.Success(paginationDto);
	}

	public async Task<ApiResponse<MerchantProductDto>> ReadProductByIdAsync(int productId)
	{
		var product = _unitOfWork.ProductCustomRepository.ReadProducts().Where(p => p.ProductId == productId);
		if (product.FirstOrDefault() == null) return ApiResponse<MerchantProductDto>.Failure("Product not found");

		var productDto = await product.Select(e => new MerchantProductDto
		{
			ProductId = e.ProductId,
			ProductName = e.ProductsName,
			ProductDescription = e.ProductDescription,
			Price = e.TblProductDetails.Select(p => p.Price).FirstOrDefault().ToString(),
			ProductCategories = e.ProductCategories.Select(p => p.Category.CategoryName).ToList(),
			ProductDetailAttributes = e.TblProductDetails.SelectMany(p => p.LkpProductDetailsAttributes).Select(p => new ProductDetailAttributeDto
			{
				Id = p.ProductDetailAttributeId,
				AttributeName = p.Attribute.Name,
				AttributeValue = p.Attribute.value,
				MeasurementUnit = p.UnitOfMeasure.UnitOfMeasureName
			}).ToList(),
			Urls = e.ProductPhotos.Select(p => new ProductPhotosDto { isMain = p.isMain, url = p.Url }).ToList()

		}).FirstOrDefaultAsync();
	

		return ApiResponse<MerchantProductDto>.Success(productDto);
	}

	public async Task<ApiResponse> UpdateProductAsync(ProductUpdateInput input)
	{
		var product =await _unitOfWork.ProductCustomRepository.ReadProductsInclude().Where(e => e.ProductId == input.productId).FirstOrDefaultAsync();
		if (product == null)
		{
			return ApiResponse.Failure("Product not found");
		}

		product.ProductsName = input.ProductName;
		product.ProductDescription = input.ProductDescription;
		var detail = product.TblProductDetails.FirstOrDefault();
		detail.Price = Convert.ToDecimal(input.Price);
		_unitOfWork.ProductDetailRepository.Update(detail);
		 _unitOfWork.ProductRepository.Update(product);

		var a = await _unitOfWork.saveAsync();
		if (a<=0)
		{
			return new ApiResponse { IsSuccess = false, Message = "Saving product after update failed" };
		}

		// Clear cache
		_cacheService.Remove($"merchant_product:{input.productId}");
		_cacheService.RemoveByPrefix($"merchant_product:list:{product.UserId}");

		_cacheService.RemoveByPrefix($"customer_product:list:all");
		_cacheService.RemoveByPrefix($"customer_product:list:shop:{product.UserId}");
		_cacheService.RemoveByPrefix($"customer_product:list:category");
		_cacheService.Remove($"customer_product:{input.productId}");


		return ApiResponse.Success("Product updated successfully");
	}

	public async Task<ApiResponse> DeleteProductAsync(int productId)
	{
		var product =await _unitOfWork.ProductCustomRepository.ReadProducts().Where(e=>e.ProductId==productId).FirstOrDefaultAsync();
		if (product == null)
		{
			return ApiResponse.Failure("Product not found");
		}

		_unitOfWork.ProductRepository.Delete(product);
		await _unitOfWork.saveAsync();

		// Clear cache
		_cacheService.Remove($"merchant_product:{productId}");
		_cacheService.RemoveByPrefix($"merchant_product:list:{product.UserId}");

		_cacheService.RemoveByPrefix($"customer_product:list:all");
		_cacheService.RemoveByPrefix($"customer_product:list:shop:{product.UserId}");
		_cacheService.RemoveByPrefix($"customer_product:list:category");
		_cacheService.Remove($"customer_product:{productId}");

		return ApiResponse.Success("Product deleted successfully");
	}

	public async Task<ApiResponse> AddProductAttributeAsync(int productId, AttributeCreateInput attributes)
	{
		//var product = await _unitOfWork.ProductRepository.ReadByIdAsync(productId);
		var product =await _unitOfWork.ProductCustomRepository.ReadProductsInclude().Where(e => e.ProductId == productId).FirstOrDefaultAsync();
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
		_unitOfWork.ProductRepository.Update(product);
		await _unitOfWork.saveAsync();
		await transaction.CommitAsync();

		_cacheService.Remove($"merchant_product:{productId}");
		_cacheService.RemoveByPrefix($"merchant_product:list:{product.UserId}");


		_cacheService.RemoveByPrefix($"customer_product:list:all");
		_cacheService.RemoveByPrefix($"customer_product:list:shop:{product.UserId}");
		_cacheService.RemoveByPrefix($"customer_product:list:category");
		_cacheService.Remove($"customer_product:{productId}");

		return ApiResponse.Success("Attributes added successfully");
	}

	public async Task<ApiResponse> RemoveProductAttributeAsync(int productId, AttributeDeleteInput attributes)
	{
		var product = await _unitOfWork.ProductCustomRepository.ReadProductsInclude().Where(e => e.ProductId == productId).FirstOrDefaultAsync();
		var productDetail = product.TblProductDetails.FirstOrDefault();
		foreach (var item in attributes.Attributes)
		{
			_unitOfWork.ProductDetailsAttributeRepository.DeleteRange(productDetail.LkpProductDetailsAttributes.Where(p => p.ProductDetailAttributeId == item));
		}

		await _unitOfWork.saveAsync();

		// Clear cache
		_cacheService.Remove($"merchant_product:{productId}");
		_cacheService.RemoveByPrefix($"merchant_product:list:{product.UserId}");


		_cacheService.RemoveByPrefix($"customer_product:list:all");
		_cacheService.RemoveByPrefix($"customer_product:list:shop:{product.UserId}");
		_cacheService.RemoveByPrefix($"customer_product:list:category");
		_cacheService.Remove($"customer_product:{productId}");

		return ApiResponse.Success("Attributes removed successfully");
	}

	public async Task<ApiResponse> AddProductCategoriesAsync(ProductCategoryInput categoryInput)
	{
		var product =await  _unitOfWork.ProductCustomRepository.ReadProductsIncludeTracking().Where(e=>e.ProductId==categoryInput.ProductId).FirstOrDefaultAsync();
		if (product == null)
			return ApiResponse.Failure("Product not found");
		foreach (var item in categoryInput.restaurantCategories)
		{
			if (product.ProductCategories.Where(c => c.categoryid == (int)item).Any())
			{
				continue;
			}
			product.ProductCategories.Add(new TblProductCategory { productid = categoryInput.ProductId, categoryid = (int)item });
		}
		var result = await _unitOfWork.saveAsync();

		// Clear cache
		_cacheService.Remove($"merchant_product:{product.ProductId}");
		_cacheService.RemoveByPrefix($"merchant_product:list:{product.UserId}");


		_cacheService.RemoveByPrefix($"customer_product:list:all");
		_cacheService.RemoveByPrefix($"customer_product:list:shop:{product.UserId}");
		_cacheService.RemoveByPrefix($"customer_product:list:category");
		_cacheService.Remove($"customer_product:{product.ProductId}");

		return ApiResponse.Success("Categories added successfully");
	}

	public async Task<ApiResponse> RemoveProductCategoriesAsync(ProductCategoryInput categoryInput)
	{
		var product = await _unitOfWork.ProductCustomRepository.ReadProductsIncludeTracking().Where(e => e.ProductId == categoryInput.ProductId).FirstOrDefaultAsync();
		if (product == null)
			return ApiResponse.Failure("Product not found");

		foreach (var item in categoryInput.restaurantCategories)
		{
			var category = product.ProductCategories
					.FirstOrDefault(c => c.categoryid == (int)item);

			if (category != null)
				product.ProductCategories.Remove(category);
		}
		await _unitOfWork.saveAsync();

		// Clear cache
		_cacheService.Remove($"merchant_product:{product.ProductId}");
		_cacheService.RemoveByPrefix($"merchant_product:list:{product.UserId}");

		_cacheService.RemoveByPrefix($"customer_product:list:all");
		_cacheService.RemoveByPrefix($"customer_product:list:shop:{product.UserId}");
		_cacheService.RemoveByPrefix($"customer_product:list:category");
		_cacheService.Remove($"customer_product:{product.ProductId}");

		return ApiResponse.Success("Categories removed successfully");
	}

	#endregion

	#region Orders

	public async Task<ApiResponse<PaginationDto<MerchantOrderDto>>> ReadAllOrdersAsync(ProductPaginationInput input)
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
			CustomerId=e.CustomerId,
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

	public async Task<ApiResponse<MerchantOrderDto>> ReadOrderByIdAsync(int orderId)
	{
		string cacheKey = $"merchant_order:{orderId}";
		var cached = _cacheService.Get<MerchantOrderDto>(cacheKey);
		if (cached != null) return ApiResponse<MerchantOrderDto>.Success(cached);

		var order =  _unitOfWork.OrderCustomRepository.ReadOrders().Where(e=>e.OrderId==orderId);
		var orderDto =await order.Select(e => new MerchantOrderDto {

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

		_cacheService.Set(cacheKey, orderDto);
		return ApiResponse<MerchantOrderDto>.Success(orderDto);

	}

	public async Task<ApiResponse> UpdateOrderStatusAsync(int orderId, OrderStatusUpdateInput input)
	{
		var order =await _unitOfWork.OrderCustomRepository.ReadOrders().Where(e => e.OrderId == orderId).FirstOrDefaultAsync();
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

	#endregion

	#region Customers

	public async Task<ApiResponse<PaginationDto<CustomerDto>>> ReadAllPurchasedCustomersAsync(ProductPaginationInput input)
	{
		var cacheKey = $"merchant_customer:list:{input.UserId}:{input.Page}:{input.PageSize}";

		// جلب من الكاش أولاً
		if (_cacheService.Get<PaginationDto<CustomerDto>>(cacheKey) is PaginationDto<CustomerDto> cachedResult)
			return ApiResponse<PaginationDto<CustomerDto>>.Success(cachedResult);

		var ordersQuery = _unitOfWork.OrderCustomRepository
			.ReadOrdersInclude()
			.Where(e => e.MerchantId == input.UserId);

		// لو مفيش أوردرز
		var totalCount = await ordersQuery.CountAsync();
		if (totalCount == 0)
		{
			var emptyResult = new PaginationDto<CustomerDto>
			{
				TotalItems = 0,
				TotalPages = 0,
				Items = new List<CustomerDto>()
			};
			_cacheService.Set(cacheKey, emptyResult);
			return ApiResponse<PaginationDto<CustomerDto>>.Success(emptyResult);
		}

		var totalPages = (int)Math.Ceiling(totalCount / (double)input.PageSize);

		// pagination للأوردرز
		var customerIds = await ordersQuery
			.Select(o => o.CustomerId)
			.Distinct()
			.Skip((input.Page - 1) * input.PageSize)
			.Take(input.PageSize)
			.ToListAsync();

		// جلب العملاء، لو مفيش يوزر مطابق، نرجع فاضي
		var customers = await _unitOfWork.UserCustomRepository
			.ReadCustomer()
			.Where(c => customerIds.Contains(c.Id))
			.ToListAsync();

		if (!customers.Any())
		{
			var emptyResult = new PaginationDto<CustomerDto>
			{
				TotalItems = 0,
				TotalPages = 0,
				Items = new List<CustomerDto>()
			};
			_cacheService.Set(cacheKey, emptyResult);
			return ApiResponse<PaginationDto<CustomerDto>>.Success(emptyResult);
		}

		// تجميع الأوردرز حسب العميل
		var groupedOrders = ordersQuery
			.Where(o => customerIds.Contains(o.CustomerId))
			.ToList()
			.GroupBy(o => o.CustomerId)
			.ToDictionary(g => g.Key, g => g.ToList());

		var customerDtos = new List<CustomerDto>();
		foreach (var c in customers)
		{
			// لو مفيش أوردرز لهذا العميل، نعديه
			if (!groupedOrders.TryGetValue(c.Id, out var custOrders) || custOrders == null || !custOrders.Any())
				continue;

			var completedOrders = custOrders.Where(o => o.OrderStatus == OrderState.Completed).ToList();

			customerDtos.Add(new CustomerDto
			{
				FullName = $"{c.TblCustomer?.FirstName ?? "Unknown"} {c.TblCustomer?.LastName ?? ""}",
				Email = c.Email ?? "N/A",
				PhoneNumber = c.PhoneNumber ?? "N/A",
				LastPurchased = completedOrders.Any() ? completedOrders.Max(o => o.OrderDate) : (DateTime?)null,
				TotalOrders = custOrders.Count,
				TotalSpent = completedOrders.Sum(o => Convert.ToDecimal(o.TotalPrice))
			});
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
