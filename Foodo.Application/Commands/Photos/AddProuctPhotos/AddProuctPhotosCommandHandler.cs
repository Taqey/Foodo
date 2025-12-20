using Foodo.Application.Abstraction.InfrastructureRelatedServices.Cache;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.Upload;
using Foodo.Application.Models.Dto.Photo;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Application.Commands.Photos.AddProuctPhotos
{
	public class AddProuctPhotosCommandHandler : IRequestHandler<AddProuctPhotosCommand, ApiResponse<List<GetPhotosDto>>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IPhotoAccessorService _photoAccessor;
		private readonly ICacheService _cacheService;

		public AddProuctPhotosCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessorService photoAccessor, ICacheService cacheService)
		{
			_unitOfWork = unitOfWork;
			_photoAccessor = photoAccessor;
			_cacheService = cacheService;
		}
		public async Task<ApiResponse<List<GetPhotosDto>>> Handle(AddProuctPhotosCommand input, CancellationToken cancellationToken)
		{
			var result = await _photoAccessor.AddProductPhotos(input.Files, input.ProductId);

			if (result == null || !result.Any(p => p.IsSuccess))
			{
				return new ApiResponse<List<GetPhotosDto>>
				{
					IsSuccess = false,
					Message = "Images uploading failed"
				};
			}

			var product = await _unitOfWork.ProductCustomRepository.ReadProducts().Where(e => e.ProductId == input.ProductId).FirstOrDefaultAsync();
			if (product == null)
			{
				return new ApiResponse<List<GetPhotosDto>>
				{
					IsSuccess = false,
					Message = "Product not found"
				};
			}

			if (product.ProductPhotos == null)
				product.ProductPhotos = new List<TblProductPhoto>();

			_unitOfWork.ProductRepository.Attach(product);
			foreach (var photo in result.Where(p => p.IsSuccess))
			{
				product.ProductPhotos.Add(new TblProductPhoto
				{
					ProductId = product.ProductId,
					Url = photo.Url,
					isMain = false
				});
			}

			await _unitOfWork.saveAsync();
			var dtoList = product.ProductPhotos
				.Select(p => new GetPhotosDto
				{
					url = p.Url,
					UrlId = p.Id
				})
				.ToList();
			_cacheService.RemoveByPrefix($"merchant_product:list:{product.UserId}");
			_cacheService.RemoveByPrefix($"customer_product:list:all");

			return new ApiResponse<List<GetPhotosDto>>
			{
				IsSuccess = true,
				Message = "Photos added successfully",
				Data = dtoList
			};
		}
	}
}
