namespace Foodo.API.Models.Request.Photo
{
	public class AddProductPhotosRequest
	{
		public IFormFileCollection Files { get; set; }
		public int ProductId { get; set; }
	}
}