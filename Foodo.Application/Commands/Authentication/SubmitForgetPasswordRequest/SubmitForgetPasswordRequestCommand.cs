using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Authentication.SubmitForgetPasswordRequest
{
	public class SubmitForgetPasswordRequestCommand : IRequest<ApiResponse>
	{
		public string Email { get; set; }

	}
}
