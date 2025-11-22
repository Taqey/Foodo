using Foodo.Application.Abstraction;
using Foodo.Application.Abstraction.Customer;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Enums;
using Foodo.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Implementation.Customer
{
	public class CustomerService : ICustomerService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUserService _userService;

		public CustomerService(IUnitOfWork unitOfWork, IUserService userService)
		{
			_unitOfWork = unitOfWork;
			_userService = userService;
		}
		public Task<ApiResponse> CancelOrder()
		{
			throw new NotImplementedException();
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
				var order = new TblOrder
				{
					UserId = input.CustomerId,
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
				createdOrder.TotalPrice = totalAmount;
				createdOrder.Tax = Tax.Apply(totalAmount);
				_unitOfWork.OrderRepository.Update(createdOrder);
				await _unitOfWork.saveAsync();
				await _unitOfWork.CommitTransactionAsync(transaction);
				return new ApiResponse
				{
					IsSuccess = true,
					Message = "Order placed successfully"
				};
			}
			catch (Exception e)
			{
				await _unitOfWork.RollbackTransactionAsync(transaction);
				return new ApiResponse
				{
					IsSuccess = false,
					Message = $"Failed to place order{e.Message}"
				};
				throw;
			}
		}

		public async Task<ApiResponse<IEnumerable<ProductDto>>> ReadAllProducts(PaginationInput input)
		{

			var result = await _unitOfWork.ProductRepository.PaginationAsync(input.Page, input.PageSize);
			var productDtos = new List<ProductDto>();
			foreach (var product in result)
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
			return new ApiResponse<IEnumerable<ProductDto>>
			{
				Data = productDtos,
				IsSuccess = true,
				Message = "Products retrieved successfully"
			};
		}

		public async Task<ApiResponse<IEnumerable<ShopDto>>> ReadAllShops(PaginationInput input)
		{
			var result = await _unitOfWork.MerchantRepository.PaginationAsync(input.Page, input.PageSize);
			var shopDtos = new List<ShopDto>();
			foreach (var shop in result)
			{
				var shopDto = new ShopDto
				{
					ShopId = shop.UserId,
					ShopName = shop.StoreName,
					ShopDescription = shop.StoreDescription,
				};
				shopDtos.Add(shopDto);


			}
			return new ApiResponse<IEnumerable<ShopDto>>
			{
				Data = shopDtos,
				IsSuccess = true,
				Message = "Shops retrieved successfully"
			};
		}

		public async Task<ApiResponse<ProductDto>> ReadProductById(ItemByIdInput input)
		{
			var product = await _unitOfWork.ProductRepository.ReadByIdAsync(Convert.ToInt32(input.ItemId));
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
				Attributes = new List<AttributeDto>(),
			};
			foreach (var pda in product.TblProductDetails.FirstOrDefault().LkpProductDetailsAttributes)
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
			return new ApiResponse<ProductDto>
			{
				Data = productDto,
				IsSuccess = true,
				Message = "Product retrieved successfully"
			};
		}

		public Task<ApiResponse> ReadProductsByCategory()
		{
			throw new NotImplementedException();
		}

		public async Task<ApiResponse<ShopDto>> ReadShopById(ItemByIdInput input)
		{
			var user = await _userService.GetByIdAsync(input.ItemId);
			var shop = user.TblMerchant;
			if (shop == null)
			{
				return ApiResponse<ShopDto>.Failure("Shop not found");
			}
			var shopDto = new ShopDto
			{
				ShopId = shop.UserId,
				ShopName = shop.StoreName,
				ShopDescription = shop.StoreDescription,
			};
			return new ApiResponse<ShopDto>
			{
				Data = shopDto,
				IsSuccess = true,
				Message = "Shop retrieved successfully"
			};
		}

		public Task<ApiResponse> ReadShopsByCategory()
		{
			throw new NotImplementedException();
		}
	}
}
