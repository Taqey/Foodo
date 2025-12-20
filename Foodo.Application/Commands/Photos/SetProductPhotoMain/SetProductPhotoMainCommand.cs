using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Photos.SetProductPhotoMain
{
	public class SetProductPhotoMainCommand : IRequest<ApiResponse>
	{
		public int id { get; set; }

	}
}
