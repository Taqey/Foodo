using Foodo.Application.Abstraction.InfrastructureRelatedServices.Mailing;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.User;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Enums;
using MediatR;
using System.Security.Cryptography;

namespace Foodo.Application.Commands.Authentication.SubmitForgetPasswordRequest
{
	public class SubmitForgetPasswordRequestCommandHandler : IRequestHandler<SubmitForgetPasswordRequestCommand, ApiResponse>
	{
		private readonly IUserService _userService;
		private readonly IEmailSenderService _senderService;

		public SubmitForgetPasswordRequestCommandHandler(IUserService userService, IEmailSenderService senderService)
		{
			_userService = userService;
			_senderService = senderService;
		}
		public async Task<ApiResponse> Handle(SubmitForgetPasswordRequestCommand input, CancellationToken cancellationToken)
		{
			var user = await _userService.GetInclude(input.Email, e => e.TblCustomer, e => e.TblMerchant);
			if (user == null)
			{
				return ApiResponse.Failure("Email not found");
			}
			var role = (await _userService.GetRolesForUser(user)).FirstOrDefault();
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
			user.LkpCodes.Add(new LkpCode { Key = code, CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddMinutes(10), CodeType = CodeType.PasswordReset });
			await _userService.UpdateAsync(user);
			var result = await _senderService.SendEmailAsync(input.Email, Name, "Password Reset", $"Your Reset Password code is {code}");
			return result;
		}
	}
}
