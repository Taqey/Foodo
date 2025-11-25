using Foodo.Application.Models.Dto.Auth;
using Foodo.Application.Models.Input.Auth;
using Foodo.Application.Models.Input.Merchant;
using Foodo.Application.Models.Response;

namespace Foodo.Application.Abstraction.Authentication
{
	public interface IAuthenticationService
	{
        Task<ApiResponse<JwtDto>> Login(LoginInput input);
		Task<ApiResponse<UserIdDto>> Register(RegisterInput input);
		Task<ApiResponse> AddCategory(CategoryInput input);
		Task<ApiResponse> ChangePassword(ChangePasswordInput input);
		Task<ApiResponse> SubmitForgetPasswordRequest(SubmitForgetPasswordRequestInput input);
		Task<ApiResponse> ForgetPassword(ForgetPasswordInput input);
		Task<ApiResponse<JwtDto>> RefreshToken(string Token);
		Task<ApiResponse> VerifyEmailRequest(VerifyEmailRequestInput input);
		Task<ApiResponse> VerifyEmail(VerifyEmailInput input);
	}
}
