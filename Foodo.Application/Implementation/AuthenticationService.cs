using Foodo.Application.Abstraction;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Implementation
{
	public class AuthenticationService : IAuthenticationService
	{
		private readonly IUserService _userService;

		public AuthenticationService(IUserService userService)
		{
			_userService = userService;
		}
		public async Task<ApiResponse> Register(RegisterInput input)
		{
			ApplicationUser? existingUser = null;
			 existingUser = await _userService.GetByEmailAsync(input.Email);
			if (existingUser!=null)
			{
				return ApiResponse.Failure("Email is already in use.");
			}
			existingUser = await _userService.GetByUsernameAsync(input.UserName);
			if (existingUser != null)
			{
				return ApiResponse.Failure("Username is already taken.");
			}

			var user=new ApplicationUser { Email=input.Email,UserName=input.UserName,PhoneNumber=input.PhoneNumber};
			string Role = "";
			if (input.UserType == UserType.Customer)
			{
				user.TblCustomer = new TblCustomer { FirstName = input.FirstName, LastName = input.LastName, Gender = input.Gender.ToString() };
				Role = "Customer";
			}
			else { 
			user.TblMerchant = new TblMerchant { StoreName = input.StoreName, StoreDescription = input.StoreDescription };
				Role = "Merchant";
			}
			IdentityResult result;
			result = await _userService.CreateUserAsync(user, input.Password);
			if (result.Succeeded)
			{
			 result=await	_userService.AddRolesToUser(user, Role);
				return ApiResponse.Success("User registered successfully.");
			}
			return ApiResponse.Failure("User registration failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
		}
		public Task Login(LoginInput input)
		{
			throw new NotImplementedException();
		}

		public Task ChangePassword(ChangePasswordInput input)
		{
			throw new NotImplementedException();
		}

		public Task ForgetPassword(ForgetPasswordInput input)
		{
			throw new NotImplementedException();
		}


	}
}
