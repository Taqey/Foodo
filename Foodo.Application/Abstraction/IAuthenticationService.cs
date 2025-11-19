using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Abstraction
{
	public interface IAuthenticationService
	{
		Task Login(LoginInput input);
        Task<ApiResponse> Register(RegisterInput input);
		Task ChangePassword(ChangePasswordInput input);
		Task ForgetPassword(ForgetPasswordInput input);
	}
}
