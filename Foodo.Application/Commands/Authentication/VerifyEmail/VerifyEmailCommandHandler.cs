using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Models.Response;
using MediatR;

namespace Foodo.Application.Commands.Authentication.VerifyEmail
{
	public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, ApiResponse>
	{
		private readonly IUserService _userService;

		public VerifyEmailCommandHandler(IUserService userService)
		{
			_userService = userService;
		}
		public async Task<ApiResponse> Handle(VerifyEmailCommand input, CancellationToken cancellationToken)
		{

			var user = await _userService.GetUserByVerificationToken(input.Code);
			if (user == null)
			{
				return new ApiResponse { Message = "Invalid or expired verification token." };
			}
			var code = user.LkpCodes.FirstOrDefault(e => e.Key == input.Code);
			if (code.ExpiresAt < DateTime.UtcNow)
			{
				return new ApiResponse { Message = "Invalid or expired verification token." };
			}
			if ((bool)code.IsUsed)
			{
				return ApiResponse.Failure("Verification token already used.");
			}
			code.IsUsed = true;
			user.EmailConfirmed = true;
			await _userService.UpdateAsync(user);

			return ApiResponse.Success("Email has been verified successfully.");
		}
	}
}
