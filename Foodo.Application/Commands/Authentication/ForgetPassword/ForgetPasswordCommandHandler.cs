using Foodo.Application.Abstraction.InfrastructureRelatedServices.User;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Authentication.ForgetPassword
{
	public class ForgetPasswordCommandHandler : IRequestHandler<ForgetPasswordCommand, ApiResponse>
	{
		private readonly IUserService _userService;

		public ForgetPasswordCommandHandler(IUserService userService)
		{
			_userService = userService;
		}
		public async Task<ApiResponse> Handle(ForgetPasswordCommand input, CancellationToken cancellationToken)
		{
			var user = await _userService.GetUserByResetCode(input.Code);
			if (user == null)
			{
				return new ApiResponse { Message = "Invalid or expired reset code." };
			}
			var code = user.LkpCodes.FirstOrDefault(e => e.Key == input.Code);
			if (code.ExpiresAt < DateTime.UtcNow)
			{
				return new ApiResponse { Message = "Invalid or expired reset code." };
			}
			if ((bool)code.IsUsed)
			{
				return ApiResponse.Failure("Reset Code Already used");
			}
			var result = await _userService.ForgetPasswordAsync(user, input.Password);
			if (result.Succeeded)
			{
				code.IsUsed = true;
				return ApiResponse.Success("Password has been reset successfully.");
			}
			return ApiResponse.Failure("Password reset failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
		}
	}
}
