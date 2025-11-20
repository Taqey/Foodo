using Foodo.Application.Models.Response;

namespace Foodo.Application.Abstraction
{
	public interface IEmailSenderService
	{
		Task<ApiResponse> SendEmailAsync(string ReceiverEmail, string ReceiverName, string subject, string body, bool isHtml = false);
	}
}
