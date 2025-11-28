using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Foodo.Application.Models.Input.Photo
{
	public class AddPhotoInput
	{
		public string Id { get; set; }
		public IFormFile file { get; set; }
		public UserType UserType { get; set; }
	}
}