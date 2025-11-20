using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Abstraction
{
	public interface IAuthenticationService
	{
        Task<ApiResponse<JwtDto>> Login(LoginInput input);
        Task<ApiResponse> Register(RegisterInput input);
		Task ChangePassword(ChangePasswordInput input);
		Task ForgetPassword(ForgetPasswordInput input);
		Task<ApiResponse<JwtDto>> RefreshToken(string Token);
	}
}
