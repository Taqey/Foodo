using Foodo.Application.Abstraction.InfrastructureRelatedServices.User;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Authentication.ChangePassword
{
	public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ApiResponse>
	{
		private readonly IUserService _userService;

		public ChangePasswordCommandHandler(IUserService userService)
		{
			_userService = userService;
		}
		public async Task<ApiResponse> Handle(ChangePasswordCommand input, CancellationToken cancellationToken)
		{
			var user = await _userService.GetByIdAsync(input.UserId);
			var result = await _userService.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);
			if (!result.Succeeded)
			{
				return ApiResponse.Failure("Password change failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
			}
			return ApiResponse.Success("Password changed successfully.");
		}
	}
}
