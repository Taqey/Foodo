using Foodo.Application.Abstraction.InfrastructureRelatedServices.Authentication;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.User;
using Foodo.Application.Models.Dto.Auth;
using Foodo.Domain.Entities;
using Foodo.Infrastructure.Helper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Foodo.Infrastructure.Services.Authentication
{
	public class CreateToken : ICreateToken
	{
		private readonly IUserService _userService;
		private readonly IOptions<JwtSettings> _options;

		public CreateToken(IUserService userService, IOptions<JwtSettings> options)
		{
			_userService = userService;
			_options = options;
		}
		public JwtDto CreateJwtToken(ApplicationUser user, string role)
		{

			var claims = new List<Claim>();
			claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
			claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
			claims.Add(new Claim(ClaimTypes.Role, role));
			var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.Key));
			var Credentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);
			var expires = DateTime.UtcNow.AddMinutes(15);
			var notBefore = DateTime.UtcNow;
			var JwtToken = new JwtSecurityToken(
				issuer: _options.Value.Issuer,
				audience: _options.Value.Audience,
				signingCredentials: Credentials,
				expires: expires,
				notBefore: notBefore,
				claims: claims
				);
			var Token = new JwtSecurityTokenHandler().WriteToken(JwtToken);
			return new JwtDto { Token = Token, CreatedOn = notBefore, ExpiresOn = expires };

		}

		public RefreshTokenDto CreatRefreshToken()
		{
			var rnd = new Byte[32];
			using var rng = new RNGCryptoServiceProvider();
			rng.GetBytes(rnd);
			var expires = DateTime.UtcNow.AddDays(7);
			var notBefore = DateTime.UtcNow;
			return new RefreshTokenDto { Token = Convert.ToBase64String(rnd), CreatedOn = notBefore, ExpiresOn = expires };


		}
	}
}
