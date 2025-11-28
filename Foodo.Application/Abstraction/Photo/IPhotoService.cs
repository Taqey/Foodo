using Foodo.Application.Models.Dto.Photo;
using Foodo.Application.Models.Input.Photo;
using Foodo.Application.Models.Response;

namespace Foodo.Application.Abstraction.Photo
{
	public interface IPhotoService
	{
		public Task<ApiResponse<GetPhotoDto>> ReadUserPhoto(GetUserPhotoInput input);

		public Task<ApiResponse> AddUserPhoto(AddPhotoInput input);
		public Task<ApiResponse<List<GetPhotosDto>>> AddProuctPhotos(AddProductPhotosInput input);
		public Task<ApiResponse> SetProductPhotoMain(SetPhotoMainInput input);


	}
}
