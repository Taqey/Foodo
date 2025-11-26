using Foodo.Application.Abstraction.Customer;
using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Input.Merchant;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Enums;
using Foodo.Domain.Repository;

namespace Foodo.Application.Implementation.Customer
{
	public class CustomerService : ICustomerService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUserService _userService;
		private readonly ICacheService _cacheService;

		public CustomerService(IUnitOfWork unitOfWork, IUserService userService, ICacheService cacheService)
		{
			_unitOfWork = unitOfWork;
			_userService = userService;
			_cacheService = cacheService;
		}

		#region Orders

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


			return ApiResponse.Success("Order cancelled successfully");
		}

		public Task<ApiResponse> EditOrder()
		{
			throw new NotImplementedException();
		}

		public async Task<ApiResponse> PlaceOrder(CreateOrderInput input)
		{
			await using var transaction = await _unitOfWork.BeginTransactionAsync();
			try
			{
				var firstItem = input.Items.First();
				var product = await _unitOfWork.ProductRepository.FindByContidtionAsync(p => p.ProductId == firstItem.ItemId);

				if (product == null)
					return ApiResponse.Failure("Product not found");

				var merchantId = product.UserId;

				var order = new TblOrder
				{
					CustomerId = input.CustomerId,
					MerchantId = merchantId,
					OrderDate = DateTime.UtcNow,
					OrderStatus = OrderState.Pending,
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

				return ApiResponse.Success("Order placed successfully");
			}
			catch (Exception e)
			{
				await _unitOfWork.RollbackTransactionAsync(transaction);
				return ApiResponse.Failure($"Failed to place order: {e.Message}");
			}
		}

		public async Task<ApiResponse<PaginationDto<CustomerOrderDto>>> ReadAllOrders(ProductPaginationInput input)
		{
			string cacheKey = $"customer_order:list:{input.UserId}:{input.Page}:{input.PageSize}";
			var cached = _cacheService.Get<PaginationDto<CustomerOrderDto>>(cacheKey);
			if (cached != null)
				return ApiResponse<PaginationDto<CustomerOrderDto>>.Success(cached);

			var (orders, totalCount, totalPages) = await _unitOfWork.OrderRepository.PaginationAsync(input.Page, input.PageSize, e => e.CustomerId == input.UserId);

			if (orders == null || !orders.Any())
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

			var merchantName = (await _unitOfWork.MerchantRepository.FindByContidtionAsync(e => e.UserId == orders.First().MerchantId)).StoreName;

			var orderDtos = orders.Select(order => new CustomerOrderDto
			{
				OrderId = order.OrderId,
				MerchantId = order.MerchantId,
				MerchantName = merchantName,
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
			}).ToList();

			var resultDto = new PaginationDto<CustomerOrderDto>
			{
				TotalItems = totalCount,
				TotalPages = totalPages,
				Items = orderDtos
			};

			_cacheService.Set(cacheKey, resultDto);
			return ApiResponse<PaginationDto<CustomerOrderDto>>.Success(resultDto);
		}

		public async Task<ApiResponse<CustomerOrderDto>> ReadOrderById(ItemByIdInput input)
		{
			string cacheKey = $"customer_order:{input.ItemId}";
			var cached = _cacheService.Get<CustomerOrderDto>(cacheKey);
			if (cached != null) return ApiResponse<CustomerOrderDto>.Success(cached);

			var order = await _unitOfWork.OrderRepository.FindByContidtionAsync(o => o.OrderId == Convert.ToInt32(input.ItemId));
			if (order == null) return ApiResponse<CustomerOrderDto>.Failure("Order not found");
			var MerchantName = (await _userService.GetByIdAsync(order.MerchantId)).TblMerchant.StoreName;
			var orderDto = new CustomerOrderDto
			{
				OrderId = order.OrderId,
				MerchantId = order.MerchantId,
				MerchantName= MerchantName,
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

			_cacheService.Set(cacheKey, orderDto);
			return ApiResponse<CustomerOrderDto>.Success(orderDto);
		}

		#endregion

		#region Products

		public async Task<ApiResponse<PaginationDto<ProductDto>>> ReadAllProducts(ProductPaginationInput input)
		{
			string cacheKey = $"customer_product:list:all:{input.Page}:{input.PageSize}";
			var cached = _cacheService.Get<PaginationDto<ProductDto>>(cacheKey);
			if (cached != null) return ApiResponse<PaginationDto<ProductDto>>.Success(cached);

			var (products, totalCount, totalPages) = await _unitOfWork.ProductRepository.PaginationAsync(input.Page, input.PageSize, null);

			if (products == null || !products.Any())
			{
				var emptyResult = new PaginationDto<ProductDto>
				{
					TotalItems = 0,
					TotalPages = 0,
					Items = new List<ProductDto>()
				};
				_cacheService.Set(cacheKey, emptyResult);
				return ApiResponse<PaginationDto<ProductDto>>.Success(emptyResult);
			}

			var productDtos = products.Select(product => new ProductDto
			{
				ProductId = product.ProductId,
				ProductName = product.ProductsName,
				ProductDescription = product.ProductDescription,
				Price = product.TblProductDetails.FirstOrDefault()?.Price.ToString() ?? "0",
				Attributes = product.TblProductDetails.FirstOrDefault()?.LkpProductDetailsAttributes.Select(pda => new AttributeDto
				{
					Name = pda.Attribute.Name,
					Value = pda.Attribute.value,
					MeasurementUnit = pda.UnitOfMeasure.UnitOfMeasureName
				}).ToList() ?? new List<AttributeDto>()
			}).ToList();

			var resultDto = new PaginationDto<ProductDto>
			{
				TotalItems = totalCount,
				TotalPages = totalPages,
				Items = productDtos
			};

			_cacheService.Set(cacheKey, resultDto);
			return ApiResponse<PaginationDto<ProductDto>>.Success(resultDto);
		}

		public async Task<ApiResponse<ProductDto>> ReadProductById(ItemByIdInput input)
		{
			string cacheKey = $"customer_product:{input.ItemId}";
			var cached = _cacheService.Get<ProductDto>(cacheKey);
			if (cached != null) return ApiResponse<ProductDto>.Success(cached);

			var product = await _unitOfWork.ProductRepository.ReadByIdAsync(Convert.ToInt32(input.ItemId));
			if (product == null) return ApiResponse<ProductDto>.Failure("Product not found");

			var productDto = new ProductDto
			{
				ProductId = product.ProductId,
				ProductName = product.ProductsName,
				ProductDescription = product.ProductDescription,
				Price = product.TblProductDetails.FirstOrDefault()?.Price.ToString() ?? "0",
				Attributes = product.TblProductDetails.FirstOrDefault()?.LkpProductDetailsAttributes.Select(pda => new AttributeDto
				{
					Name = pda.Attribute.Name,
					Value = pda.Attribute.value,
					MeasurementUnit = pda.UnitOfMeasure.UnitOfMeasureName
				}).ToList() ?? new List<AttributeDto>()
			};

			_cacheService.Set(cacheKey, productDto);
			return ApiResponse<ProductDto>.Success(productDto);
		}

		public async Task<ApiResponse<PaginationDto<ProductDto>>> ReadProductsByCategory(ProductPaginationByCategoryInput input)
		{
			string cacheKey = $"customer_product:list:category:{input.Category}:{input.Page}:{input.PageSize}";
			var cached = _cacheService.Get<PaginationDto<ProductDto>>(cacheKey);
			if (cached != null) return ApiResponse<PaginationDto<ProductDto>>.Success(cached);

			var (products, totalCount, totalPages) = await _unitOfWork.ProductRepository.PaginationAsync(input.Page, input.PageSize, p => p.ProductCategory.Any(c => c.categoryid == (int)input.Category));

			var productDtos = products.Select(product => new ProductDto
			{
				ProductId = product.ProductId,
				ProductName = product.ProductsName,
				ProductDescription = product.ProductDescription,
				Price = product.TblProductDetails.FirstOrDefault()?.Price.ToString() ?? "0",
				Attributes = product.TblProductDetails.FirstOrDefault()?.LkpProductDetailsAttributes.Select(pda => new AttributeDto
				{
					Name = pda.Attribute.Name,
					Value = pda.Attribute.value,
					MeasurementUnit = pda.UnitOfMeasure.UnitOfMeasureName
				}).ToList() ?? new List<AttributeDto>()
			}).ToList();

			var resultDto = new PaginationDto<ProductDto>
			{
				TotalItems = totalCount,
				TotalPages = totalPages,
				Items = productDtos
			};

			_cacheService.Set(cacheKey, resultDto);
			return ApiResponse<PaginationDto<ProductDto>>.Success(resultDto);
		}

		public async Task<ApiResponse<PaginationDto<ProductDto>>> ReadProductsByShop(ProductPaginationByShopInput input)
		{
			string cacheKey = $"customer_product:list:shop:{input.MerchantId}:{input.Page}:{input.PageSize}";
			var user =await _userService.GetByIdAsync(input.MerchantId);
			var (products, totalCount, totalPages) = await _unitOfWork.ProductRepository.PaginationAsync(input.Page,input.PageSize,e=>e.UserId==input.MerchantId);
			if (products == null || !products.Any())
			{
				var emptyResult = new PaginationDto<ProductDto>
				{
					TotalItems = 0,
					TotalPages = 0,
					Items = new List<ProductDto>()
				};
				_cacheService.Set(cacheKey, emptyResult);
				return ApiResponse<PaginationDto<ProductDto>>.Success(emptyResult);
			}

			var productDtos = products.Select(product => new ProductDto
			{
				ProductId = product.ProductId,
				ProductName = product.ProductsName,
				ProductDescription = product.ProductDescription,
				Price = product.TblProductDetails.FirstOrDefault()?.Price.ToString() ?? "0",

				Attributes = product.TblProductDetails.FirstOrDefault()?.LkpProductDetailsAttributes.Select(pda => new AttributeDto
				{
					Name = pda.Attribute.Name,
					Value = pda.Attribute.value,
					MeasurementUnit = pda.UnitOfMeasure.UnitOfMeasureName
				}).ToList() ?? new List<AttributeDto>(),

				ProductCategories = product.ProductCategory
	.Select(c => ((FoodCategory)c.categoryid).ToString())
	.ToList()

			}).ToList();


			var resultDto = new PaginationDto<ProductDto>
			{
				TotalItems = totalCount,
				TotalPages = totalPages,
				Items = productDtos
			};
			_cacheService.Set(cacheKey, resultDto);
			return ApiResponse<PaginationDto<ProductDto>>.Success(resultDto);
		}
		#endregion

		#region Shops

		public async Task<ApiResponse<PaginationDto<ShopDto>>> ReadAllShops(ProductPaginationInput input)
		{
			string cacheKey = $"customer_merchant:list:all:{input.Page}:{input.PageSize}";
			var cached = _cacheService.Get<PaginationDto<ShopDto>>(cacheKey);
			if (cached != null) return ApiResponse<PaginationDto<ShopDto>>.Success(cached);

			var (shops, totalCount, totalPages) = await _unitOfWork.MerchantRepository.PaginationAsync(input.Page, input.PageSize, null);

			var shopDtos = shops.Select(shop => new ShopDto
			{
				ShopId = shop.UserId,
				ShopName = shop.StoreName,
				ShopDescription = shop.StoreDescription,
				Categories = shop.TblRestaurantCategories
	.Select(c => ((RestaurantCategory)c.categoryid).ToString())
	.ToList()

			}).ToList();

			var resultDto = new PaginationDto<ShopDto>
			{
				TotalItems = totalCount,
				TotalPages = totalPages,
				Items = shopDtos
			};

			_cacheService.Set(cacheKey, resultDto);
			return ApiResponse<PaginationDto<ShopDto>>.Success(resultDto);
		}

		public async Task<ApiResponse<ShopDto>> ReadShopById(ItemByIdInput input)
		{
			string cacheKey = $"customer_merchant:{input.ItemId}";
			var cached = _cacheService.Get<ShopDto>(cacheKey);
			if (cached != null) return ApiResponse<ShopDto>.Success(cached);

			var user = await _userService.GetByIdAsync(input.ItemId);
			var shop = user.TblMerchant;
			if (shop == null) return ApiResponse<ShopDto>.Failure("Shop not found");

			var shopDto = new ShopDto
			{
				ShopId = shop.UserId,
				ShopName = shop.StoreName,
				ShopDescription = shop.StoreDescription,
						Categories = shop.TblRestaurantCategories
.Select(c => ((RestaurantCategory)c.categoryid).ToString())
.ToList()
			};


			_cacheService.Set(cacheKey, shopDto);
			return ApiResponse<ShopDto>.Success(shopDto);
		}

		public async Task<ApiResponse<PaginationDto<ShopDto>>> ReadShopsByCategory(ShopsPaginationByCategoryInput input)
		{
			string cacheKey = $"customer_merchant:list:category:{input.Category}:{input.Page}:{input.PageSize}";
			var cached = _cacheService.Get<PaginationDto<ShopDto>>(cacheKey);
			if (cached != null) return ApiResponse<PaginationDto<ShopDto>>.Success(cached);

			var (shops, totalCount, totalPages) = await _unitOfWork.MerchantRepository.PaginationAsync(input.Page, input.PageSize, m => m.TblRestaurantCategories.Any(c => c.categoryid == (int)input.Category));

			if (shops == null || !shops.Any())
			{
				var emptyResult = new PaginationDto<ShopDto>
				{
					TotalItems = 0,
					TotalPages = 0,
					Items = new List<ShopDto>()
				};
				_cacheService.Set(cacheKey, emptyResult);
				return ApiResponse<PaginationDto<ShopDto>>.Success(emptyResult);
			}

			var shopDtos = shops.Select(shop => new ShopDto
			{
				ShopId = shop.UserId,
				ShopName = shop.StoreName,
				ShopDescription = shop.StoreDescription,
				Categories = shop.TblRestaurantCategories
	.Select(c => ((RestaurantCategory)c.categoryid).ToString())
	.ToList()

			}).ToList();

			var resultDto = new PaginationDto<ShopDto>
			{
				TotalItems = totalCount,
				TotalPages = totalPages,
				Items = shopDtos
			};

			_cacheService.Set(cacheKey, resultDto);
			return ApiResponse<PaginationDto<ShopDto>>.Success(resultDto);
		}


		#endregion
	}
}
