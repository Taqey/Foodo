using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Application.Commands.Products.AddProductAttribute
{
	public class AddProductAttributeCommandHandler : IRequestHandler<AddProductAttributeCommand, ApiResponse>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICacheService _cacheService;

		public AddProductAttributeCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
		{
			_unitOfWork = unitOfWork;
			_cacheService = cacheService;
		}
		public async Task<ApiResponse> Handle(AddProductAttributeCommand request, CancellationToken cancellationToken)
		{
			var product = await _unitOfWork.ProductCustomRepository.ReadProductsInclude().Where(e => e.ProductId == request.ProductId).FirstOrDefaultAsync();
			if (product == null)
				return ApiResponse.Failure("Product not found");

			var productDetail = product.TblProductDetails.FirstOrDefault();
			if (productDetail == null)
				return ApiResponse.Failure("Product details not found");

			using var transaction = await _unitOfWork.BeginTransactionAsync();

			foreach (var item in request.attributes.Attributes)
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

			//_cacheService.Remove($"merchant_product:{productId}");
			_cacheService.RemoveByPrefix($"merchant_product:list:{product.UserId}");
			_cacheService.RemoveByPrefix($"customer_product:list:all");
			_cacheService.RemoveByPrefix($"customer_product:list:shop:{product.UserId}");
			_cacheService.RemoveByPrefix($"customer_product:list:category");
			//_cacheService.Remove($"customer_product:{productId}");

			return ApiResponse.Success("Attributes added successfully");
		}
	}
}
