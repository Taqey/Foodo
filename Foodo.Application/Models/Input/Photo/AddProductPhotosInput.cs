using Microsoft.AspNetCore.Http;

namespace Foodo.Application.Models.Input.Photo
{
	public class AddProductPhotosInput
	{
		public IFormFileCollection Files { get; set; }
		public int ProductId { get; set; }
	}
}