using Foodo.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace Foodo.Application.Abstraction.InfraRelated
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
		Task<ApplicationUser> GetUserByResetToken(string token);
		Task<ApplicationUser> GetUserByRefreshToken(string token);
		Task<ApplicationUser> GetUserByVerificationToken(string token);
		Task<IdentityResult> UpdateAsync(ApplicationUser user);
		Task<IdentityResult> AddRolesToUser(ApplicationUser user, string Role);
		Task<IList<string>> GetRolesForUser(ApplicationUser user);
		Task<ApplicationUser> GetUserByResetCode(string Code);
		Task<ApplicationUser> GetInclude(string email, params Expression<Func<ApplicationUser, object>>[] includeProperties);

	}
}
