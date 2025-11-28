using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Foodo.Application.Abstraction.InfrastructureRelatedServices;
using Foodo.Application.Models.Dto.Photo;
using Foodo.Application.Models.Input.Photo;
using Foodo.Domain.Enums;
using Foodo.Infrastructure.Helper;
using Microsoft.Extensions.Options;

namespace Foodo.Infrastructure.Services
{
	public class PhotoAccessorService : IPhotoAccessorService
	{
		private readonly Cloudinary _cloudinary;
		private readonly IOptions<CloudinarySettings> _options;

		public PhotoAccessorService(IOptions<CloudinarySettings> options)
		{
			_options = options;
			var account = new Account
			{
				Cloud = options.Value.CloudName,
				ApiKey = options.Value.ApiKey,
				ApiSecret = options.Value.ApiSecret
			};
			_cloudinary = new Cloudinary(account);
		}
		public async Task<PhotoResultDto> AddPhoto(AddPhotoInput input)
		{

			if (input.file.Length == 0 || input.file == null)
			{
				return new PhotoResultDto { Message = "file is empty" };
			}
			await using var stream = input.file.OpenReadStream();
			ImageUploadParams uploadParameters;
			if (input.UserType == UserType.Customer)
			{
				uploadParameters = new ImageUploadParams
				{
					File = new FileDescription(input.file.FileName, stream),
					Folder = "Foodo_Customers"
				};

			}
			else
			{
				uploadParameters = new ImageUploadParams
				{
					File = new FileDescription(input.file.FileName, stream),
					Folder = "Foodo_Merchants"
				};
			}
			var result = await _cloudinary.UploadAsync(uploadParameters);
			if (result.Error != null)
			{
				return new PhotoResultDto { Message = "Upload Failed" };
			}
			return new PhotoResultDto { IsSuccess = true, Message = "Uploaded Successfully", PublicId = result.PublicId, Url = result.SecureUrl.ToString() };
		}
		public async Task<string> DeletePhoto(string publicId)
		{
			var deleteParams=new DeletionParams(publicId);
			var result= await _cloudinary.DestroyAsync(deleteParams);
			return result.Result.ToString();
		}
		public async Task<List<PhotoResultDto>> AddProductPhotos(AddProductPhotosInput input)
		{
			var photos = new List<PhotoResultDto>();

			if (input.Files == null || input.Files.Count == 0)
			{
				photos.Add(new PhotoResultDto { Message = "No files received" });
				return photos;
			}

			foreach (var file in input.Files)
			{
				if (file == null || file.Length == 0)
				{
					photos.Add(new PhotoResultDto { Message = "One of the files is empty" });
					continue;
				}

				await using var stream = file.OpenReadStream();

				var uploadParams = new ImageUploadParams
				{
					File = new FileDescription(file.FileName, stream),
					Folder = "Foodo_Products"
				};

				var result = await _cloudinary.UploadAsync(uploadParams);

				if (result.Error != null)
				{
					photos.Add(new PhotoResultDto { Message = "Upload failed for a file" });
					continue;
				}

				photos.Add(new PhotoResultDto
				{
					IsSuccess = true,
					Message = "Uploaded Successfully",
					PublicId = result.PublicId,
					Url = result.SecureUrl.ToString()
				});
			}

			return photos;
		}

	}
}
