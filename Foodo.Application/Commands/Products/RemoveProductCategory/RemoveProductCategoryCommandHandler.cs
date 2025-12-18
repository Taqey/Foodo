using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Models.Response;
using Foodo.Domain.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Application.Commands.Products.RemoveProductCategory
{
	public class RemoveProductCategoryCommandHandler : IRequestHandler<RemoveProductCategoryCommand, ApiResponse>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICacheService _cacheService;

		public RemoveProductCategoryCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
		{
			_unitOfWork = unitOfWork;
			_cacheService = cacheService;
		}
		public async Task<ApiResponse> Handle(RemoveProductCategoryCommand request, CancellationToken cancellationToken)
		{
			var product = await _unitOfWork.ProductCustomRepository.ReadProductsIncludeTracking().Where(e => e.ProductId == request.categoryInput.ProductId).FirstOrDefaultAsync();
			if (product == null)
				return ApiResponse.Failure("Product not found");

			foreach (var item in request.categoryInput.restaurantCategories)
			{
				var category = product.ProductCategories
						.FirstOrDefault(c => c.categoryid == (int)item);

				if (category != null)
					product.ProductCategories.Remove(category);
			}
			await _unitOfWork.saveAsync();
			// Clear cache
			//_cacheService.Remove($"merchant_product:{product.ProductId}");
			_cacheService.RemoveByPrefix($"merchant_product:list:{product.UserId}");
			_cacheService.RemoveByPrefix($"customer_product:list:all");
			_cacheService.RemoveByPrefix($"customer_product:list:shop:{product.UserId}");
			_cacheService.RemoveByPrefix($"customer_product:list:category");
			//_cacheService.Remove($"customer_product:{product.ProductId}");
			return ApiResponse.Success("Categories removed successfully");
		}
	}
}
