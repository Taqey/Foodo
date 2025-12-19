using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Authentication.ChangePassword
{
	public class ChangePasswordCommand : IRequest<ApiResponse>
	{
		public string CurrentPassword { get; set; }
		public string NewPassword { get; set; }
		public string UserId { get; set; }
	}
}
