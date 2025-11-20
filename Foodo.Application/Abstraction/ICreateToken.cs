using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Abstraction
{
	public interface ICreateToken
	{
		JwtDto CreateJwtToken(ApplicationUser user, string role);
		RefreshTokenDto CreatRefreshToken();
	}
}
