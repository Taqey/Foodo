using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Enums;
using MediatR;
using System.Security.Cryptography;

namespace Foodo.Application.Commands.Authentication.VerifyEmailRequest
{
	public class VerifyEmailRequestCommandHandler : IRequestHandler<VerifyEmailRequestCommand, ApiResponse>
	{
		private readonly IUserService _userService;
		private readonly IEmailSenderService _senderService;

		public VerifyEmailRequestCommandHandler(IUserService userService, IEmailSenderService senderService)
		{
			_userService = userService;
			_senderService = senderService;
		}
		public async Task<ApiResponse> Handle(VerifyEmailRequestCommand input, CancellationToken cancellationToken)
		{
			var user = await _userService.GetInclude(input.Email, e => e.TblCustomer, e => e.TblMerchant);
			if (user == null)
			{
				return ApiResponse.Failure("Email not found");
			}
			var role = input.Role;
			string Name;
			if (role == (UserType.Merchant).ToString())
			{
				Name = user.TblMerchant.StoreName;
			}
			else
			{
				Name = user.TblCustomer.FirstName;
			}
			var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
			user.LkpCodes.Add(new LkpCode { Key = code, CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddMinutes(10), CodeType = CodeType.EmailVerification });
			await _userService.UpdateAsync(user);

			var result = _senderService.SendEmailAsync(input.Email, "User", "Verify your email", $"Your Email Verification code is {code}");
			return ApiResponse.Success("Verification email sent.");
		}
	}
}
