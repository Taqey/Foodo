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
		public async Task<ApiResponse> CancelOrder(ItemByIdInput input)
		{
			var order =await _unitOfWork.OrderRepository.FindByContidtionAsync(o => o.OrderId == Convert.ToInt32(input.ItemId));
			if (order == null)
			{
				return ApiResponse.Failure("Order not found");
			}
			order.OrderStatus = OrderState.Cancelled;
			var result=await _unitOfWork.saveAsync();
			if (result <= 0)
			{
				return ApiResponse.Failure("Failed to cancel order");
			}
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
				{
					return new ApiResponse
					{
						IsSuccess = false,
						Message = "Product not found"
					};
				}

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
					Message = $"Failed to place order: {e.Message}"
				};
			}
		}


		public async Task<ApiResponse<IEnumerable<OrderDto>>> ReadAllOrders(PaginationInput input)
		{
			var orders =await  _unitOfWork.OrderRepository.PaginationAsync(input.Page, input.PageSize,e=>e.CustomerId==input.FilterBy);
			var MerchantName=(await _unitOfWork.MerchantRepository.FindByContidtionAsync(e=>e.UserId==orders.FirstOrDefault().MerchantId)).StoreName;
			var orderDtos = new List<OrderDto>();
			foreach (var order in orders) {
				var orderDto = new OrderDto
				{
					OrderId = order.OrderId,
					MerchantId = order.MerchantId,
					MerchantName=MerchantName,
					CustomerId = order.CustomerId,
					OrderDate = order.OrderDate,
					TotalAmount = order.TotalPrice,
					Status = (order.OrderStatus).ToString(),
					OrderItems = new List<OrderItemDto>()
				};
				foreach (var item in order.TblProductsOrders)
				{
					var orderItemDto = new OrderItemDto
					{
						ItemName = item.Product.ProductsName,
						ItemId = item.ProductId,
						Quantity = item.Quantity,
						Price = item.Price,
					};
					orderDto.OrderItems.Add(orderItemDto);
				}
				orderDtos.Add(orderDto);
			}
			return new ApiResponse<IEnumerable<OrderDto>>
			{
				Data = orderDtos,
				IsSuccess = true,
				Message = "Orders retrieved successfully"
			};
		}

		public async Task<ApiResponse<IEnumerable<ProductDto>>> ReadAllProducts(PaginationInput input)
		{

			var result = await _unitOfWork.ProductRepository.PaginationAsync(input.Page, input.PageSize,null);
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
			var result = await _unitOfWork.MerchantRepository.PaginationAsync(input.Page, input.PageSize,null);
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

		public async Task<ApiResponse<OrderDto>> ReadOrderById(ItemByIdInput input)
		{
			var order =await _unitOfWork.OrderRepository.FindByContidtionAsync(o => o.OrderId == Convert.ToInt32(input.ItemId));
			OrderDto orderDto=new OrderDto
			{
				OrderId = order.OrderId,
				MerchantId=order.MerchantId,
				CustomerId = order.CustomerId,
				OrderDate = order.OrderDate,
				TotalAmount = order.TotalPrice,
				Status = (order.OrderStatus).ToString(),
				OrderItems = new List<OrderItemDto>()

			};
			foreach (var item in order.TblProductsOrders)
			{
				var orderItemDto = new OrderItemDto
				{
					ItemName= item.Product.ProductsName,
					ItemId = item.ProductId,
					Quantity = item.Quantity,
					Price = item.Price,
				};
				orderDto.OrderItems.Add(orderItemDto);
			}
			return new ApiResponse<OrderDto>
			{
				Data = orderDto,
				IsSuccess = true,
				Message = "Order retrieved successfully"
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
