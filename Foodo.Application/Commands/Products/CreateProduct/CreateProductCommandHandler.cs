using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Models.Dto.Merchant;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using MediatR;

namespace Foodo.Application.Commands.Products.CreateProduct
{
	public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ApiResponse<CreateProductDto>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICacheService _cacheService;

		public CreateProductCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
		{
			_unitOfWork = unitOfWork;
			_cacheService = cacheService;
		}
		public async Task<ApiResponse<CreateProductDto>> Handle(CreateProductCommand input, CancellationToken cancellationToken)
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
	}
}
