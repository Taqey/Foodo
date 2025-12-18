using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Application.Commands.Products.AddProductCategory
{
	public class AddProductCategoryCommandHandler : IRequestHandler<AddProductCategoryCommand, ApiResponse>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICacheService _cacheService;

		public AddProductCategoryCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
		{
			_unitOfWork = unitOfWork;
			_cacheService = cacheService;
		}
		public async Task<ApiResponse> Handle(AddProductCategoryCommand request, CancellationToken cancellationToken)
		{
			var product = await _unitOfWork.ProductCustomRepository.ReadProductsIncludeTracking().Where(e => e.ProductId == request.input.ProductId).FirstOrDefaultAsync();
			if (product == null)
				return ApiResponse.Failure("Product not found");
			foreach (var item in request.input.restaurantCategories)
			{
				if (product.ProductCategories.Where(c => c.categoryid == (int)item).Any())
				{
					continue;
				}
				product.ProductCategories.Add(new TblProductCategory { productid = request.input.ProductId, categoryid = (int)item });
			}
			var result = await _unitOfWork.saveAsync();
			// Clear cache
			//_cacheService.Remove($"merchant_product:{product.ProductId}");
			_cacheService.RemoveByPrefix($"merchant_product:list:{product.UserId}");
			_cacheService.RemoveByPrefix($"customer_product:list:all");
			_cacheService.RemoveByPrefix($"customer_product:list:shop:{product.UserId}");
			_cacheService.RemoveByPrefix($"customer_product:list:category");
			//_cacheService.Remove($"customer_product:{product.ProductId}");
			return ApiResponse.Success("Categories added successfully");
		}
	}
}
