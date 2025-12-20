using Foodo.Application.Abstraction.InfrastructureRelatedServices.Cache;
using Foodo.Application.Models.Response;
using Foodo.Domain.Repository;
using MediatR;

namespace Foodo.Application.Commands.Products.DeleteProduct
{
	public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, ApiResponse>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICacheService _cacheService;

		public DeleteProductCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
		{
			_unitOfWork = unitOfWork;
			_cacheService = cacheService;
		}
		public async Task<ApiResponse> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
		{
			var product = await _unitOfWork.ProductRepository.ReadByIdAsync(request.productId);
			if (product == null)
			{
				return ApiResponse.Failure("Product not found");
			}
			product.IsDeleted = true;
			//product.DeletedBy = request.UserId;
			await _unitOfWork.saveAsync();

			_cacheService.RemoveByPrefix($"merchant_product:list:{product.UserId}");
			_cacheService.RemoveByPrefix($"customer_product:list:all");
			return ApiResponse.Success("Product deleted successfully");
		}
	}
}
