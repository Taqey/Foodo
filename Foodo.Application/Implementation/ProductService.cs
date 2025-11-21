using Foodo.Application.Abstraction.Merchant;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Implementation;

public class ProductService : IProductService
{
	private readonly IUnitOfWork _unitOfWork;

	public ProductService(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}
	public async Task<ApiResponse> CreateProductAsync(ProductInput input)
	{
		var Productresult = await _unitOfWork.ProductRepository.CreateAsync(new TblProduct
		{
			UserId=input.Id,
			ProductsName = input.ProductName,
			ProductDescription = input.ProductDescription,
		});
		await _unitOfWork.saveAsync();

		var ProductDetailResult =await _unitOfWork.ProductDetailRepository.CreateAsync(new TblProductDetail
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
		foreach (var item in attributes.Zip(measureUnits,(a,b)=>new {a,b}))
		{

			await _unitOfWork.ProductDetailsAttributeRepository.CreateAsync(new LkpProductDetailsAttribute {AttributeId=item.a.AttributeId,UnitOfMeasureId=item.b.UnitOfMeasureId,ProductDetailId= ProductDetailResult.ProductDetailId });
		}
		await _unitOfWork.saveAsync();

		return ApiResponse.Success("Product created successfully");

	}

	public async Task<ApiResponse<List<ProductDto>>> ReadAllProductsAsync(string UserId)
	{
		var products = await _unitOfWork.ProductRepository.FindAllByContidtionAsync(p=> p.UserId== UserId);
		var productDtos = new List<ProductDto>();
		foreach (var product in products)
		{
			var productDto = new ProductDto
			{
				ProducId= product.ProductId,
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
		return ApiResponse<List<ProductDto>>.Success(productDtos);
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
			ProducId= product.ProductId,
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
		var product =await  _unitOfWork.ProductRepository.ReadByIdAsync(productId);
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

		var a=await _unitOfWork.saveAsync();

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

		return ApiResponse.Success("Product deleted successfully");
	}

	public async Task<ApiResponse> AddProductAttributeAsync(int productId, AttributeCreateInput attributes)
	{
		var product=await _unitOfWork.ProductRepository.ReadByIdAsync(productId);
		var prodctDetail= product.TblProductDetails.FirstOrDefault();
		foreach (var item in attributes.Attributes)
		{
			var attribute= await _unitOfWork.AttributeRepository.CreateAsync( new LkpAttribute { Name=item.Name,value=item.Value});
			await _unitOfWork.saveAsync();
			var measureUnit= await _unitOfWork.MeasureUnitRepository.CreateAsync(new LkpMeasureUnit { UnitOfMeasureName=item.MeasurementUnit});
			await _unitOfWork.saveAsync();
			await _unitOfWork.ProductDetailsAttributeRepository.CreateAsync(new LkpProductDetailsAttribute { AttributeId=attribute.AttributeId,UnitOfMeasureId=measureUnit.UnitOfMeasureId,ProductDetailId= prodctDetail.ProductDetailId});
			await _unitOfWork.saveAsync();
		}
		return ApiResponse.Success("Attributes added successfully");
	}

	public async Task<ApiResponse> RemoveProductAttributeAsync(int productId, AttributeDeleteInput attributes)
	{
		var product = await _unitOfWork.ProductRepository.ReadByIdAsync(productId);
		var productDetail = product.TblProductDetails.FirstOrDefault();
		foreach (var item in attributes.Attributes) {
		_unitOfWork.ProductDetailsAttributeRepository.DeleteRange(productDetail.LkpProductDetailsAttributes.Where(p => p.ProductDetailAttributeId == item));
		}

		await _unitOfWork.saveAsync();
		return ApiResponse.Success("Attributes removed successfully");
	}
}
