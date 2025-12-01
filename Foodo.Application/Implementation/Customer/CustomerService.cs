using Foodo.Application.Abstraction.Customer;
using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Application.Models.Dto.Photo;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Input.Merchant;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Enums;
using Foodo.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using System.Net;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
			_cacheService.RemoveByPrefix($"merchant_customer:list:{order.MerchantId}");



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
				//var product = await _unitOfWork.ProductRepository.FindByContidtionAsync(p => p.ProductId == firstItem.ItemId);
				var product = await _unitOfWork.ProductCustomRepository.ReadProducts().Where(p => p.ProductId == firstItem.ItemId).Select(e =>new { e.Merchant.UserId}).FirstOrDefaultAsync(); 
				if (product == null)
					return ApiResponse.Failure("Product not found");

				var merchantId = product.UserId;
				var customerAdress =await _unitOfWork.UserCustomRepository.ReadCustomer().Where(e=>e.Id==input.CustomerId).SelectMany(e=>e.TblAdresses).Where(e=>e.IsDefault==true).Select(e=>e.AddressId).FirstOrDefaultAsync();
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

		public async Task<ApiResponse<PaginationDto<CustomerOrderDto>>> ReadAllOrders(ProductPaginationInput input)
		{
			string cacheKey = $"customer_order:list:{input.UserId}:{input.Page}:{input.PageSize}";
			var cached = _cacheService.Get<PaginationDto<CustomerOrderDto>>(cacheKey);
			if (cached != null)
				return ApiResponse<PaginationDto<CustomerOrderDto>>.Success(cached);

			var OrderQuery = _unitOfWork.OrderCustomRepository.ReadOrdersInclude().Where(e => e.CustomerId == input.UserId);
			var totalCount = await OrderQuery.CountAsync();
			var totalPages = (int)Math.Ceiling(totalCount / (double)input.PageSize);
			var FilterOrderQuery = OrderQuery.Skip((input.Page-1)*input.PageSize).Take(input.PageSize);
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
			var orders =await FilterOrderQuery.AsAsyncEnumerable().Select(e => new CustomerOrderDto
			{
				OrderId = e.OrderId,
				MerchantId = e.MerchantId,
				MerchantName = e.TblProductsOrders.Select(p => p.Product.Merchant.StoreName).FirstOrDefault(),
				OrderDate = e.OrderDate,
				TotalAmount = e.TotalPrice,
				Status = e.OrderStatus.ToString(),
				billingAddress = customerAddresses
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

		public async Task<ApiResponse<CustomerOrderDto>> ReadOrderById(ItemByIdInput input)
		{
			string cacheKey = $"customer_order:{input.ItemId}";

			var cached = _cacheService.Get<CustomerOrderDto>(cacheKey);
			if (cached != null)
				return ApiResponse<CustomerOrderDto>.Success(cached);

			int orderId = Convert.ToInt32(input.ItemId);

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


			string billingAddress =(await _unitOfWork.AdressRepository.ReadByIdAsync(orderId)).StreetAddress;

			// --------- Build Final DTO ----------
			var orderDto = new CustomerOrderDto
			{
				OrderId = orderData.OrderId,
				MerchantId = orderData.MerchantId,
				MerchantName = orderData.MerchantName,
				OrderDate = orderData.OrderDate,
				TotalAmount = orderData.TotalPrice,
				Status = orderData.Status,
				billingAddress = billingAddress,
				OrderItems = orderData.OrderItems
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

			var query = _unitOfWork.ProductCustomRepository.ReadProducts();

			var totalCount = await query.CountAsync();
			if (totalCount == 0)
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
			var totalPages = (int)Math.Ceiling(totalCount / (double)input.PageSize);

			var productDtos = await query
				.Select(e => new ProductDto
				{
					ProductId = e.ProductId,
					ProductName = e.ProductsName,
					ProductDescription = e.ProductDescription,
					Price = e.TblProductDetails
								//.OrderBy(pd => pd.ProductId) // لو في ترتيب معين
								.Select(pd => pd.Price.ToString())
								.FirstOrDefault() ?? "0",
					Attributes = e.TblProductDetails
								//.OrderBy(pd => pd.ProductId)
								.SelectMany(pd => pd.LkpProductDetailsAttributes)
								.Select(pda => new AttributeDto
								{
									Name = pda.Attribute.Name,
									Value = pda.Attribute.value,
									MeasurementUnit = pda.UnitOfMeasure.UnitOfMeasureName
								}).ToList(),
					Urls = e.ProductPhotos
								.Select(p => new ProductPhotosDto
								{
									url = p.Url,
									isMain = p.isMain
								}).ToList(),
					ProductCategories = e.ProductCategories
						.Select(pc => pc.Category.CategoryName)
						.ToList()
				})
				.Skip((input.Page - 1) * input.PageSize)
				.Take(input.PageSize)
				.ToListAsync();
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
			var query = _unitOfWork.ProductCustomRepository.ReadProducts();
			var productDto = await query
		.Select(e => new ProductDto
		{
			ProductId = e.ProductId,
			ProductName = e.ProductsName,
			ProductDescription = e.ProductDescription,
			Price = e.TblProductDetails
						.OrderBy(pd => pd.ProductId)
						.Select(pd => pd.Price.ToString())
						.FirstOrDefault() ?? "0",
			Attributes = e.TblProductDetails
						.OrderBy(pd => pd.ProductId)
						.SelectMany(pd => pd.LkpProductDetailsAttributes)
						.Select(pda => new AttributeDto
						{
							Name = pda.Attribute.Name,
							Value = pda.Attribute.value,
							MeasurementUnit = pda.UnitOfMeasure.UnitOfMeasureName
						}).ToList(),
			Urls = e.ProductPhotos
						.Select(p => new ProductPhotosDto
						{
							url = p.Url,
							isMain = p.isMain
						}).ToList(),
			ProductCategories = e.ProductCategories
						.Select(pc => pc.Category.CategoryName)
						.ToList()
		})
		.FirstOrDefaultAsync(e => e.ProductId == Convert.ToInt32(input.ItemId));

			if (productDto == null) return ApiResponse<ProductDto>.Failure("Product not found");



			_cacheService.Set(cacheKey, productDto);
			return ApiResponse<ProductDto>.Success(productDto);
		}

		public async Task<ApiResponse<PaginationDto<ProductDto>>> ReadProductsByCategory(ProductPaginationByCategoryInput input)
		{
			string cacheKey = $"customer_product:list:category:{input.Category}:{input.Page}:{input.PageSize}";
			var cached = _cacheService.Get<PaginationDto<ProductDto>>(cacheKey);
			if (cached != null) return ApiResponse<PaginationDto<ProductDto>>.Success(cached);

			var query = _unitOfWork.ProductCustomRepository.ReadProducts();
			var filteredQuery = query.Where(p => p.ProductCategories.Any(c => c.categoryid == (int)input.Category));
			var totalCount = await filteredQuery.CountAsync();
			var totalPages = (int)Math.Ceiling(totalCount / (decimal)input.PageSize);
			var productDtos = await filteredQuery
				.Select(e => new ProductDto
				{
					ProductId = e.ProductId,
					ProductName = e.ProductsName,
					ProductDescription = e.ProductDescription,
					Price = (e.TblProductDetails.Single().Price).ToString(),
					Attributes = e.TblProductDetails.SelectMany(pd => pd.LkpProductDetailsAttributes).Select(pda => new AttributeDto
					{
						Name = pda.Attribute.Name,
						Value = pda.Attribute.value,
						MeasurementUnit = pda.UnitOfMeasure.UnitOfMeasureName
					}).ToList(),
					ProductCategories = e.ProductCategories.Select(c => c.Category.CategoryName).ToList(),
					Urls = e.ProductPhotos.Select(pp => new ProductPhotosDto
					{
						url = pp.Url,
						isMain = pp.isMain
					}).ToList()

				})
				.Skip((input.Page - 1) * input.PageSize)
				.Take(input.PageSize)
				.ToListAsync();

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
			var user = await _userService.GetByIdAsync(input.MerchantId);
			var query = _unitOfWork.ProductCustomRepository.ReadProducts();
			var filteredQuery = query.Where(e => e.UserId == input.MerchantId);
			var totalCount = await filteredQuery.CountAsync();
			var totalPages = (int)Math.Ceiling(totalCount / (decimal)input.PageSize);
			var productDtos = await filteredQuery
				.Select(e => new ProductDto
				{
					ProductId = e.ProductId,
					ProductName = e.ProductsName,
					ProductDescription = e.ProductDescription,
					Price = (e.TblProductDetails.Single().Price).ToString(),
					Attributes = e.TblProductDetails.SelectMany(pd => pd.LkpProductDetailsAttributes).Select(pda => new AttributeDto
					{
						Name = pda.Attribute.Name,
						Value = pda.Attribute.value,
						MeasurementUnit = pda.UnitOfMeasure.UnitOfMeasureName
					}).ToList(),
					ProductCategories = e.ProductCategories.Select(c => c.Category.CategoryName).ToList(),
					Urls = e.ProductPhotos.Select(pp => new ProductPhotosDto
					{
						url = pp.Url,
						isMain = pp.isMain
					}).ToList()

				})
				.Skip((input.Page - 1) * input.PageSize)
				.Take(input.PageSize)
				.ToListAsync();


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

			var query = _unitOfWork.RestaurantCustomRepository.ReadRestaurants();
			var totalCount = await query.CountAsync();
			if (totalCount == 0)
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
			var totalPages = (int)Math.Ceiling(totalCount / (double)input.PageSize);
			var shopDtos = await query
				.Select(e => new ShopDto
				{
					ShopId = e.UserId,
					ShopName = e.StoreName,
					ShopDescription = e.StoreDescription,
					Categories = e.TblRestaurantCategories
					.Select(c => ((RestaurantCategory)c.categoryid).ToString())
					.ToList(),
					url=e.User.UserPhoto.Url ?? null
				})
				.Skip((input.Page - 1) * input.PageSize)
				.Take(input.PageSize)
				.ToListAsync();

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
			var query = _unitOfWork.RestaurantCustomRepository.ReadRestaurants();
			var filteredQuery = query.Where(e => e.UserId == input.ItemId);
			var shopDto =await filteredQuery.Select(e => new ShopDto
			{
				ShopId = e.UserId,
				ShopName = e.StoreName,
				ShopDescription = e.StoreDescription,
				Categories = e.TblRestaurantCategories
					.Select(c => ((RestaurantCategory)c.categoryid).ToString())
					.ToList(),
				url = e.User.UserPhoto.Url ?? null
			}).FirstOrDefaultAsync();

			if (shopDto == null) return ApiResponse<ShopDto>.Failure("Shop not found");


			_cacheService.Set(cacheKey, shopDto);
			return ApiResponse<ShopDto>.Success(shopDto);
		}

		public async Task<ApiResponse<PaginationDto<ShopDto>>> ReadShopsByCategory(ShopsPaginationByCategoryInput input)
		{
			string cacheKey = $"customer_merchant:list:category:{input.Category}:{input.Page}:{input.PageSize}";
			var cached = _cacheService.Get<PaginationDto<ShopDto>>(cacheKey);
			if (cached != null) return ApiResponse<PaginationDto<ShopDto>>.Success(cached);
			var query = _unitOfWork.RestaurantCustomRepository.ReadRestaurants();
			var filteredQuery = query.Where(e=>e.TblRestaurantCategories.Any(c => c.categoryid == (int)input.Category));
			var totalCount = await filteredQuery.CountAsync();
			if (totalCount == 0)
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
			var totalPages = (int)Math.Ceiling(totalCount / (double)input.PageSize);
			var shopDtos = await filteredQuery
				.Select(e => new ShopDto
				{
					ShopId = e.UserId,
					ShopName = e.StoreName,
					ShopDescription = e.StoreDescription,
					Categories = e.TblRestaurantCategories
					.Select(c => ((RestaurantCategory)c.categoryid).ToString())
					.ToList(),
					url = e.User.UserPhoto.Url ?? null
				})
				.Skip((input.Page - 1) * input.PageSize)
				.Take(input.PageSize)
				.ToListAsync();
			//var (shops, totalCount, totalPages) = await _unitOfWork.MerchantRepository.PaginationAsync(input.Page, input.PageSize, m => m.TblRestaurantCategories.Any(c => c.categoryid == (int)input.Category));



			//		var shopDtos = shops.Select(shop => new ShopDto
			//		{
			//			ShopId = shop.UserId,
			//			ShopName = shop.StoreName,
			//			ShopDescription = shop.StoreDescription,
			//			Categories = shop.TblRestaurantCategories
			//.Select(c => ((RestaurantCategory)c.categoryid).ToString())
			//.ToList(),
			//			url = shop.User?.UserPhoto?.Url ?? null

			//		}).ToList();

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
