using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Abstraction.InfrastructureRelatedServices;
using Foodo.Application.Abstraction.Photo;
using Foodo.Application.Models.Dto.Photo;
using Foodo.Application.Models.Input.Photo;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using Microsoft.EntityFrameworkCore;
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
		private readonly ICacheService _cacheService;

		public PhotoService(IPhotoAccessorService photoAccessor,IUserService userService,IUnitOfWork unitOfWork,ICacheService cacheService)
		{
			_photoAccessor = photoAccessor;
			_userService = userService;
			_unitOfWork = unitOfWork;
			_cacheService = cacheService;
		}


		public async Task<ApiResponse> AddUserPhoto(AddPhotoInput input)
		{
			var user = await _unitOfWork.UserCustomRepository.ReadMerchants().Where(e => e.Id == input.Id).FirstOrDefaultAsync();
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

			// 1️⃣ أضف الصور بدون إنشاء DTO الآن
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

			// 2️⃣ احفظ أولاً لتوليد IDs
			await _unitOfWork.saveAsync();

			// 3️⃣ الآن أنشئ الـ DTO باستخدام IDs الصحيحة
			var dtoList = product.ProductPhotos
				.Select(p => new GetPhotosDto
				{
					url = p.Url,
					UrlId = p.Id
				})
				.ToList();
			_cacheService.RemoveByPrefix($"merchant_product:list:{product.UserId}");
			_cacheService.RemoveByPrefix($"customer_product:list:all");
			_cacheService.RemoveByPrefix($"customer_product:list:shop:{product.UserId}");
			_cacheService.RemoveByPrefix($"customer_product:list:category");
			return new ApiResponse<List<GetPhotosDto>>
			{
				IsSuccess = true,
				Message = "Photos added successfully",
				Data = dtoList
			};
		}


		public async Task<ApiResponse> SetProductPhotoMain(SetPhotoMainInput input)
		{
			// 1) هات الصورة المطلوبة (Tracked)
			var photo = await _unitOfWork.productPhotoCustomRepository.ReadPhotos().Where(e=>e.Id == input.id).FirstOrDefaultAsync();
			if (photo == null)
			{
				return new ApiResponse
				{
					IsSuccess = false,
					Message = "Photo not found"
				};
			}

			// 2) هات باقي الصور (Tracked برضه)
			var productPhotos = await _unitOfWork.productPhotoCustomRepository
				.ReadPhotos().Where(p => p.ProductId == photo.ProductId).ToListAsync();

			// 3) خلي كل الصور isMain = false
			foreach (var p in productPhotos)
			{
				p.isMain = false;
				// ❌ بدون Update(p)
			}

			// 4) خلي الصورة المطلوبة هي الـ Main
			photo.isMain = true;
			// ❌ بدون Update(photo)

			// 5) حفظ التعديلات
			await _unitOfWork.saveAsync();

			// 6) Clear Cache
			_cacheService.RemoveByPrefix($"merchant_product:list:{photo.TblProduct.UserId}");
			_cacheService.RemoveByPrefix($"customer_product:list:all");
			_cacheService.RemoveByPrefix($"customer_product:list:shop:{photo.TblProduct.UserId}");
			_cacheService.RemoveByPrefix($"customer_product:list:category");

			return new ApiResponse
			{
				IsSuccess = true,
				Message = "Image set as main successfully"
			};
		}




		public async Task<ApiResponse<GetPhotoDto>> ReadUserPhoto(GetUserPhotoInput input)
		{
			var user = await _unitOfWork.UserCustomRepository
				.ReadMerchants()
				.Where(e => e.Id == input.Id)
				.FirstOrDefaultAsync();

			if (user == null)
			{
				return new ApiResponse<GetPhotoDto>
				{
					IsSuccess = false,
					Message = "User not found"
				};
			}

			// check if photo exists
			if (user.UserPhoto == null || string.IsNullOrEmpty(user.UserPhoto.Url))
			{
				return new ApiResponse<GetPhotoDto>
				{
					IsSuccess = false,
					Message = "Image not found"
				};
			}

			var dto = new GetPhotoDto { url = user.UserPhoto.Url };

			return new ApiResponse<GetPhotoDto>
			{
				Data = dto,
				Message = "Image retrieved",
				IsSuccess = true
			};
		}

	}
}
