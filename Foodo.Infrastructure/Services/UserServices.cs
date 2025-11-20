using Foodo.Application.Abstraction;
using Foodo.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Infrastructure.Services
{
	public class UserService : IUserService
	{
		private readonly UserManager<ApplicationUser> _manager;

		public UserService(UserManager<ApplicationUser> manager)
		{
			_manager = manager;
		}
		public async Task<ApplicationUser?> GetByEmailAsync(string email)
		{

			var user = await _manager.FindByEmailAsync(email);
			return user;

		}
		public async Task<ApplicationUser?> GetByIdAsync(string id)
		{

			var user = await _manager.FindByIdAsync(id);
			return user;

		}
		public async Task<ApplicationUser?> GetByUsernameAsync(string Name)
		{
			var user = await _manager.FindByNameAsync(Name);
			return user;
		}
		public async Task<IdentityResult> CreateUserAsync(ApplicationUser applicationUser, string password)
		{
			var result = await _manager.CreateAsync(applicationUser, password);
			
			return result;

		}
		public async Task<bool> CheckPasswordAsync(ApplicationUser applicationUser, string Password)
		{
			var result = await _manager.CheckPasswordAsync(applicationUser, Password);
			return result;

		}
		public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser applicationUser, string OldPassword, string Newpassword)
		{
			var result = await _manager.ChangePasswordAsync(applicationUser, OldPassword, Newpassword);
			return result;

		}
		public async Task<IdentityResult> ForgetPasswordAsync(ApplicationUser applicationUser, string password)
		{
			var token = await _manager.GeneratePasswordResetTokenAsync(applicationUser);
			var result = await _manager.ResetPasswordAsync(applicationUser, token, password);
			return result;
		}
		public async Task UpdateAsync(ApplicationUser user)
		{
			await _manager.UpdateAsync(user);

		}
		public async Task<IdentityResult> AddRolesToUser(ApplicationUser user, string Role)
		{
			
			var result=await _manager.AddToRoleAsync(user, Role);
			return result;
		}
		public async Task<IList<string>> GetRolesForUser(ApplicationUser user)
		{
			var roles = await _manager.GetRolesAsync(user);
			return roles;
		}
		public async Task<ApplicationUser> GetUserByToken(string token)
		{
			var user=await _manager.Users.Include(e=>e.lkpRefreshTokens).SingleOrDefaultAsync(e=>e.lkpRefreshTokens.Any(t=>t.Token==token));
			return user;
		}



	}
}
