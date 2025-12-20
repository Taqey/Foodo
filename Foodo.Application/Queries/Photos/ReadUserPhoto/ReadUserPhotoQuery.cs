using Foodo.Application.Models.Dto.Photo;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Queries.Photos.ReadUserPhoto
{
	public class ReadUserPhotoQuery : IRequest<ApiResponse<GetPhotoDto>>
	{
		public string Id { get; set; }

	}
}
