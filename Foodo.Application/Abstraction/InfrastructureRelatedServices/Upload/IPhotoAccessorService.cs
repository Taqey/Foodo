using Foodo.Application.Models.Dto.Photo;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.Upload
{
	public interface IPhotoAccessorService
	{
		Task<PhotoResultDto> AddPhoto(string Id, UserType UserType, IFormFile file);
		Task<List<PhotoResultDto>> AddProductPhotos(IFormFileCollection Files, int ProductId);
		Task<string> DeletePhoto(string publicId);
	}
}
