using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Domain.Entities;
using Foodo.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
		public async Task<ApplicationUser> GetUserByResetToken(string token)
		{
			var user = await _manager.Users.Include(e => e.LkpCodes).SingleOrDefaultAsync(e => e.LkpCodes.Any(t => t.Key == token && t.CodeType == CodeType.PasswordReset));
			return user;
		}
		public async Task<ApplicationUser> GetUserByToken(string token)
		{
			return await _manager.Users
				.SingleOrDefaultAsync(x => x.lkpRefreshTokens != null && x.lkpRefreshTokens.Any(rt => rt.Token == token));
		}
		public async Task<ApplicationUser> GetUserByVerificationToken(string token)
		{
			var user = await _manager.Users.Include(e => e.LkpCodes).SingleOrDefaultAsync(e => e.LkpCodes.Any(t => t.Key == token && t.CodeType == CodeType.EmailVerification));
			return user;
		}
		public async Task<ApplicationUser> GetUserByResetCode(string Code)
		{
			var user = await _manager.Users.Include(e=>e.LkpCodes).SingleOrDefaultAsync(e=>e.LkpCodes.Any(t=>t.Key== Code));
			return user;

		}
		public async Task<ApplicationUser> GetInclude(string email,params Expression<Func<ApplicationUser, object>>[] includeProperties)
		{
			var query = _manager.Users.AsQueryable();
			foreach (var includeProperty in includeProperties)
			{
				query = query.Include(includeProperty);
			}
			return await query.SingleOrDefaultAsync(e=>e.Email==email);
		}


	}
}
