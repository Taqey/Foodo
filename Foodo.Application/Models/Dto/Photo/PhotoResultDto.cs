namespace Foodo.Application.Models.Dto.Photo
{
	public class PhotoResultDto
	{
		public string? PublicId { get; set; }
		public string? Url { get; set; }
		public bool IsSuccess { get; set; } = false;
		public string Message { get; set; }

	}
}