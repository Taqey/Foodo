using Foodo.Application.Models.Dto.Photo;
using Foodo.Application.Models.Input.Photo;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices
{
	public interface IPhotoAccessorService
	{
		Task<PhotoResultDto> AddPhoto(AddPhotoInput input);
		Task<List<PhotoResultDto>> AddProductPhotos(AddProductPhotosInput input);
		Task<string> DeletePhoto(string publicId);
	}
}
