using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Models.Response;
using Foodo.Domain.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Application.Commands.Products.UpdateProduct
{
	public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ApiResponse>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICacheService _cacheService;

		public UpdateProductCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
		{
			_unitOfWork = unitOfWork;
			_cacheService = cacheService;
		}
		public async Task<ApiResponse> Handle(UpdateProductCommand input, CancellationToken cancellationToken)
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

			//_cacheService.Remove($"merchant_product:{input.productId}");
			_cacheService.RemoveByPrefix($"merchant_product:list:{product.UserId}");
			_cacheService.RemoveByPrefix($"customer_product:list:all");
			_cacheService.RemoveByPrefix($"customer_product:list:shop:{product.UserId}");
			_cacheService.RemoveByPrefix($"customer_product:list:category");
			//_cacheService.Remove($"customer_product:{input.productId}");


			return ApiResponse.Success("Product updated successfully");
		}
	}
}
