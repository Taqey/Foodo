using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Abstraction.Product;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Merchant;
using Foodo.Application.Models.Dto.Photo;
using Foodo.Application.Models.Dto.Product;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Input.Merchant;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Implementation.Product
{
	public class MerchantProductService : IMerchantProductService
	{
		private readonly ICacheService _cacheService;
		private readonly IUnitOfWork _unitOfWork;

		public MerchantProductService(ICacheService cacheService , IUnitOfWork unitOfWork)
		{
			_cacheService = cacheService;
			_unitOfWork = unitOfWork;
		}

		#region Create

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

		#endregion

		#region Read

		public async Task<ApiResponse<MerchantProductDto>> ReadProduct(ItemByIdInput input)
		{
			var product = _unitOfWork.ProductCustomRepository.ReadProducts().Where(p => p.ProductId == Convert.ToInt32(input.ItemId));
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
		public async Task<ApiResponse<PaginationDto<MerchantProductDto>>> ReadProducts(ProductPaginationInput input)
		{
			string cacheKey = $"merchant_product:list:{input.UserId}:{input.Page}:{input.PageSize}";

			var cached = _cacheService.Get<PaginationDto<MerchantProductDto>>(cacheKey);
			if (cached != null)
			{
				return ApiResponse<PaginationDto<MerchantProductDto>>.Success(cached);
			}
			var products = _unitOfWork.ProductCustomRepository.ReadProducts().Where(p => p.UserId == input.UserId).Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
			var totalCount = await products.CountAsync();
			if (totalCount == 0)
			{
				return new ApiResponse<PaginationDto<MerchantProductDto>> { IsSuccess = true, Message = "No products found" };
			}
			var totalPages = (int)Math.Ceiling((decimal)totalCount / input.PageSize);
			var productDtos = await products.Select(e => new MerchantProductDto
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

		#endregion

		#region Update

		public async Task<ApiResponse> UpdateProductAsync(ProductUpdateInput input)
		{
			var product = await _unitOfWork.ProductCustomRepository.ReadProductsInclude().Where(e => e.ProductId == input.productId).FirstOrDefaultAsync();
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
			if (a <= 0)
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
		public async Task<ApiResponse> AddProductAttributeAsync(int productId, AttributeCreateInput attributes)
		{
			var product = await _unitOfWork.ProductCustomRepository.ReadProductsInclude().Where(e => e.ProductId == productId).FirstOrDefaultAsync();
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
		public async Task<ApiResponse> AddProductCategoriesAsync(ProductCategoryInput categoryInput)
		{
			var product = await _unitOfWork.ProductCustomRepository.ReadProductsIncludeTracking().Where(e => e.ProductId == categoryInput.ProductId).FirstOrDefaultAsync();
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

		#region Delete

		public async Task<ApiResponse> DeleteProductAsync(int productId)
		{
			var product = await _unitOfWork.ProductCustomRepository.ReadProducts().Where(e => e.ProductId == productId).FirstOrDefaultAsync();
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
		
		#endregion

	}
}
