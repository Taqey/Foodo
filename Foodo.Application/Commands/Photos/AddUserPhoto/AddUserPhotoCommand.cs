using Foodo.Application.Models.Response;
using Foodo.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Foodo.Application.Commands.Photos.AddUserPhoto
{
	public class AddUserPhotoCommand : IRequest<ApiResponse>
	{
		public string Id { get; set; }
		public IFormFile file { get; set; }
		public UserType UserType { get; set; }
	}
}
