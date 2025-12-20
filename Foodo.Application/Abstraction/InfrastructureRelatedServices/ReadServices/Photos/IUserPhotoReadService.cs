using Foodo.Application.Models.Dto.Photo;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Photos
{
	public interface IUserPhotoReadService
	{
		Task<GetPhotoDto> ReadUserPhoto(string Id);
	}
}
