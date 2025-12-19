using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Authentication.VerifyEmail
{
	public class VerifyEmailCommand : IRequest<ApiResponse>
	{
		public string Code { get; set; }

	}
}
