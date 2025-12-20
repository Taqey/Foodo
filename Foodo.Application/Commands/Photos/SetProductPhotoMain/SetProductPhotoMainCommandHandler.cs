using Foodo.Application.Abstraction.InfrastructureRelatedServices.Cache;
using Foodo.Application.Models.Response;
using Foodo.Domain.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Application.Commands.Photos.SetProductPhotoMain
{
	public class SetProductPhotoMainCommandHandler : IRequestHandler<SetProductPhotoMainCommand, ApiResponse>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICacheService _cacheService;

		public SetProductPhotoMainCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
		{
			_unitOfWork = unitOfWork;
			_cacheService = cacheService;
		}
		public async Task<ApiResponse> Handle(SetProductPhotoMainCommand input, CancellationToken cancellationToken)
		{
			var photo = await _unitOfWork.productPhotoCustomRepository.ReadPhotos().Where(e => e.Id == input.id).FirstOrDefaultAsync();
			if (photo == null)
			{
				return new ApiResponse
				{
					IsSuccess = false,
					Message = "Photo not found"
				};
			}

			var productPhotos = await _unitOfWork.productPhotoCustomRepository
				.ReadPhotos().Where(p => p.ProductId == photo.ProductId).ToListAsync();

			foreach (var p in productPhotos)
			{
				p.isMain = false;
			}

			photo.isMain = true;

			await _unitOfWork.saveAsync();

			_cacheService.RemoveByPrefix($"merchant_product:list:{photo.TblProduct.UserId}");
			_cacheService.RemoveByPrefix($"customer_product:list:all");

			return new ApiResponse
			{
				IsSuccess = true,
				Message = "Image set as main successfully"
			};
		}
	}
}
