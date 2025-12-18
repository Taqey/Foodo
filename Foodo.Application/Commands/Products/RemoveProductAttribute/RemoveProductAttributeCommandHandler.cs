using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Models.Response;
using Foodo.Domain.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Application.Commands.Products.RemoveProductAttribute
{
	public class RemoveProductAttributeCommandHandler : IRequestHandler<RemoveProductAttributeCommand, ApiResponse>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICacheService _cacheService;

		public RemoveProductAttributeCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
		{
			_unitOfWork = unitOfWork;
			_cacheService = cacheService;
		}
		public async Task<ApiResponse> Handle(RemoveProductAttributeCommand request, CancellationToken cancellationToken)
		{
			var product = await _unitOfWork.ProductCustomRepository.ReadProductsInclude().Where(e => e.ProductId == request.ProductId).FirstOrDefaultAsync();
			var productDetail = product.TblProductDetails.FirstOrDefault();
			foreach (var item in request.attributes.Attributes)
			{
				_unitOfWork.ProductDetailsAttributeRepository.DeleteRange(productDetail.LkpProductDetailsAttributes.Where(p => p.ProductDetailAttributeId == item));
			}

			await _unitOfWork.saveAsync();

			// Clear cache
			//_cacheService.Remove($"merchant_product:{productId}");
			_cacheService.RemoveByPrefix($"merchant_product:list:{product.UserId}");
			_cacheService.RemoveByPrefix($"customer_product:list:all");
			_cacheService.RemoveByPrefix($"customer_product:list:shop:{product.UserId}");
			_cacheService.RemoveByPrefix($"customer_product:list:category");
			//_cacheService.Remove($"customer_product:{productId}");

			return ApiResponse.Success("Attributes removed successfully");
		}
	}
}
