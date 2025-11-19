using Foodo.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Abstraction
{
	public interface IUserService
	{
		Task<ApplicationUser?> GetByEmailAsync(string email);
		Task<ApplicationUser?> GetByIdAsync(string id);
		Task<ApplicationUser?> GetByUsernameAsync(string username);
		Task<IdentityResult> CreateUserAsync(ApplicationUser applicationUser, string password);
		Task<bool> CheckPasswordAsync(ApplicationUser applicationUser, string Password);
		Task<IdentityResult> ChangePasswordAsync(ApplicationUser applicationUser, string oldPassword, string newPassword);
		Task<IdentityResult> ForgetPasswordAsync(ApplicationUser applicationUser, string newPassword);
		//Task<ApplicationUser> GetUserByToken(string token);
		Task UpdateAsync(ApplicationUser user);
		Task<IdentityResult> AddRolesToUser(ApplicationUser user, string Role);

	}
}
