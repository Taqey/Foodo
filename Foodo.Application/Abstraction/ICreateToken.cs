using Foodo.Application.Models.Dto;
using Foodo.Domain.Entities;

namespace Foodo.Application.Abstraction
{
	public interface ICreateToken
	{
		JwtDto CreateJwtToken(ApplicationUser user, string role);
		RefreshTokenDto CreatRefreshToken();
	}
}
