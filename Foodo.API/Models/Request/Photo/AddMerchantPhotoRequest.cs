namespace Foodo.API.Models.Request.Photo
{
	public class AddMerchantPhotoRequest
	{
		public IFormFile file { get; set; }
		public string MerchantId { get; set; }
	}
}