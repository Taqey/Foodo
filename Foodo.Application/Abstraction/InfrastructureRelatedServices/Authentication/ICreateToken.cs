using Foodo.Application.Models.Dto.Auth;
using Foodo.Domain.Entities;

namespace Foodo.Application.Abstraction.InfrastructureRelatedServices.Authentication
{
	public interface ICreateToken
	{
		JwtDto CreateJwtToken(ApplicationUser user, string role);
		RefreshTokenDto CreatRefreshToken();
	}
}
