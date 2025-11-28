using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Abstraction.InfrastructureRelatedServices;
using Foodo.Application.Abstraction.Photo;
using Foodo.Application.Models.Dto.Photo;
using Foodo.Application.Models.Input.Photo;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Implementation.Photo
{
	public class PhotoService : IPhotoService
	{
		private readonly IPhotoAccessorService _photoAccessor;
		private readonly IUserService _userService;
		private readonly IUnitOfWork _unitOfWork;

		public PhotoService(IPhotoAccessorService photoAccessor,IUserService userService,IUnitOfWork unitOfWork)
		{
			_photoAccessor = photoAccessor;
			_userService = userService;
			_unitOfWork = unitOfWork;
		}


		public async Task<ApiResponse> AddUserPhoto(AddPhotoInput input)
		{
			var user = await _userService.GetByIdAsync(input.Id);
			var result=await _photoAccessor.AddPhoto(input);
			if (!result.IsSuccess)
			{
				return new ApiResponse { IsSuccess = result.IsSuccess, Message = result.Message };
			}
			if (user.UserPhoto == null)
				user.UserPhoto = new LkpUserPhoto();
			user.UserPhoto.Url = result.Url;
			var UpdateResult=await _userService.UpdateAsync(user);
			if (!UpdateResult.Succeeded)
			{
				return new ApiResponse { IsSuccess = false, Message = "Adding Image to DB failed" };
			}
			return new ApiResponse { IsSuccess = true, Message = "Image Added successfully" };

		}

		public async Task<ApiResponse<List<GetPhotosDto>>> AddProuctPhotos(AddProductPhotosInput input)
		{
			var result = await _photoAccessor.AddProductPhotos(input);

			if (result == null || !result.Any(p => p.IsSuccess))
			{
				return new ApiResponse<List<GetPhotosDto>>
				{
					IsSuccess = false,
					Message = "Images uploading failed"
				};
			}

			var product = await _unitOfWork.ProductRepository.ReadByIdAsync(input.ProductId);
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

			var dtoList = new List<GetPhotosDto>();

			foreach (var photo in result.Where(p => p.IsSuccess))
			{
				var newPhoto = new TblProductPhoto
				{
					ProductId = product.ProductId,
					Url = photo.Url,
					isMain = false
				};

				product.ProductPhotos.Add(newPhoto);

				// اضبط الـ DTO بعد الحفظ عشان ناخد الـ Id من قاعدة البيانات
				dtoList.Add(new GetPhotosDto
				{
					url = newPhoto.Url,
					UrlId = newPhoto.Id
				});
			}

			await _unitOfWork.saveAsync();

			return new ApiResponse<List<GetPhotosDto>>
			{
				IsSuccess = true,
				Message = "Photos added successfully",
				Data = dtoList
			};
		}


		public async Task<ApiResponse> SetProductPhotoMain(SetPhotoMainInput input)
		{
			var photo = await _unitOfWork.ProductPhotoRepository.ReadByIdAsync(input.id);
			if (photo == null)
			{
				return new ApiResponse { IsSuccess = false, Message = "Photo not found" };
			}

			var productPhotos = await _unitOfWork.ProductPhotoRepository
				.FindAllByContidtionAsync(p => p.ProductId == photo.ProductId && p.isMain);

			foreach (var p in productPhotos)
			{
				p.isMain = false;
			}

			photo.isMain = true;

			await _unitOfWork.saveAsync();

			return new ApiResponse { IsSuccess = true, Message = "Image set as main successfully" };
		}


		public async Task<ApiResponse<GetPhotoDto>> ReadUserPhoto(GetUserPhotoInput input)
		{
			var user = await _userService.GetByIdAsync(input.Id);
			if (user==null)
			{
				return new ApiResponse<GetPhotoDto> { IsSuccess = false, Message = "User not found" };
			}
			var url = user.UserPhoto.Url;
			if (string.IsNullOrEmpty(url))
			{
				return new ApiResponse<GetPhotoDto> { IsSuccess = false, Message = "Image not found" };
			}
			var dto = new GetPhotoDto { url=url};
			return new ApiResponse<GetPhotoDto> { Data=dto,Message="Image retrived",IsSuccess=true };
		}
	}
}
