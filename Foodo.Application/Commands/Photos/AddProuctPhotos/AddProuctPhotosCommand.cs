using Foodo.Application.Models.Dto.Photo;
using Foodo.Application.Models.Response;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Foodo.Application.Commands.Photos.AddProuctPhotos
{
	public class AddProuctPhotosCommand : IRequest<ApiResponse<List<GetPhotosDto>>>
	{
		public IFormFileCollection Files { get; set; }
		public int ProductId { get; set; }
	}
}
