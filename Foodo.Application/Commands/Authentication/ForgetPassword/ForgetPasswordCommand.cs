using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Authentication.ForgetPassword
{
	public class ForgetPasswordCommand : IRequest<ApiResponse>
	{
		public string Code { get; set; }
		public string Password { get; set; }
	}
}
