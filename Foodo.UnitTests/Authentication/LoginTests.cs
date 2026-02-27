using Foodo.Application.Abstraction.InfrastructureRelatedServices.Authentication;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.User;
using Foodo.Application.Commands.Authentication.Login;
using Foodo.Application.Models.Dto.Auth;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Foodo.UnitTests.Authentication
{
	[TestFixture]
	public class LoginTests
	{
		private Mock<IUserService> _userServiceMock = null!;
		private Mock<ICreateToken> _createTokenMock = null!;
		private DefaultHttpContext _httpContext = null!;
		private LoginCommandHandler _handler = null!;

		[SetUp]
		public void Setup()
		{
			_userServiceMock = new Mock<IUserService>();
			_createTokenMock = new Mock<ICreateToken>();
			_httpContext = new DefaultHttpContext();
			var httpAccessorMock = new Mock<IHttpContextAccessor>();
			httpAccessorMock.SetupGet(h => h.HttpContext).Returns(_httpContext);

			_handler = new LoginCommandHandler(_userServiceMock.Object, _createTokenMock.Object, httpAccessorMock.Object);
		}

		[Test]
		public async Task Handle_EmailNotFound_ReturnsInvalidEmailOrUsernameFailure()
		{
			// Arrange
			var input = new LoginCommand { Email = "nonexistent@example.com", Password = "Pwd123!" };
			_userServiceMock.Setup(s => s.GetByEmailAsync(input.Email)).ReturnsAsync((ApplicationUser?)null);

			// Act
			var result = await _handler.Handle(input, CancellationToken.None);

			// Assert
			ClassicAssert.IsFalse(result.IsSuccess);
			ClassicAssert.AreEqual("Invalid email or username.", result.Message);
			ClassicAssert.IsNull(result.Data);
		}

		[Test]
		public async Task Handle_UsernameNotFound_ReturnsInvalidEmailOrUsernameFailure()
		{
			// Arrange
			var input = new LoginCommand { Email = "someuser", Password = "Pwd123!" }; // no '@' -> username path
			_userServiceMock.Setup(s => s.GetByUsernameAsync(input.Email)).ReturnsAsync((ApplicationUser?)null);

			// Act
			var result = await _handler.Handle(input, CancellationToken.None);

			// Assert
			ClassicAssert.IsFalse(result.IsSuccess);
			ClassicAssert.AreEqual("Invalid email or username.", result.Message);
			ClassicAssert.IsNull(result.Data);
		}

		[Test]
		public async Task Handle_InvalidPassword_ReturnsInvalidPasswordFailure()
		{
			// Arrange
			var input = new LoginCommand { Email = "user@example.com", Password = "BadPwd" };
			var user = new ApplicationUser { Id = "id1", Email = input.Email, UserName = "user" };

			_userServiceMock.Setup(s => s.GetByEmailAsync(input.Email)).ReturnsAsync(user);
			_userServiceMock.Setup(s => s.CheckPasswordAsync(user, input.Password)).ReturnsAsync(false);

			// Act
			var result = await _handler.Handle(input, CancellationToken.None);

			// Assert
			ClassicAssert.IsFalse(result.IsSuccess);
			ClassicAssert.AreEqual("Invalid password.", result.Message);
			ClassicAssert.IsNull(result.Data);
		}

		[Test]
		public async Task Handle_ValidCredentials_ReturnsSuccessAndSetsCookieAndRefreshToken()
		{
			// Arrange
			var input = new LoginCommand { Email = "user@example.com", Password = "GoodPwd!1" };
			var user = new ApplicationUser { Id = "id1", Email = input.Email, UserName = "user" };

			_userServiceMock.Setup(s => s.GetByEmailAsync(input.Email)).ReturnsAsync(user);
			_userServiceMock.Setup(s => s.CheckPasswordAsync(user, input.Password)).ReturnsAsync(true);
			_userServiceMock.Setup(s => s.GetRolesForUser(user)).ReturnsAsync(new List<string> { "Customer" } as IList<string>);
			_userServiceMock.Setup(s => s.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

			var jwt = new JwtDto { Token = "access-token", CreatedOn = DateTime.UtcNow, ExpiresOn = DateTime.UtcNow.AddMinutes(15) };
			var refresh = new RefreshTokenDto { Token = "refresh-token", CreatedOn = DateTime.UtcNow, ExpiresOn = DateTime.UtcNow.AddDays(7) };

			_createTokenMock.Setup(c => c.CreateJwtToken(user, "Customer")).Returns(jwt);
			_createTokenMock.Setup(c => c.CreatRefreshToken()).Returns(refresh);

			// Act
			var result = await _handler.Handle(input, CancellationToken.None);

			// Assert
			ClassicAssert.IsTrue(result.IsSuccess, "Expected success for valid credentials");
			ClassicAssert.AreEqual("Login successful.", result.Message);
			ClassicAssert.IsNotNull(result.Data);
			ClassicAssert.AreEqual(jwt.Token, result.Data.Token);

			// Cookie should be set in response headers
			var setCookie = _httpContext.Response.Headers["Set-Cookie"].ToString();
			StringAssert.Contains("RefreshToken=", setCookie);
			StringAssert.Contains("refresh-token", setCookie);

			// Refresh token added to user
			ClassicAssert.IsNotEmpty(user.lkpRefreshTokens);
			ClassicAssert.IsTrue(user.lkpRefreshTokens.Any(rt => rt.Token == refresh.Token));

			_userServiceMock.Verify(s => s.UpdateAsync(user), Times.Once);
		}

		[Test]
		public async Task CreateTokens_AddsRefreshTokenToUser_UpdatesUser_ReturnsJwtDto()
		{
			// Arrange
			var user = new ApplicationUser { Id = "idA", Email = "a@a.com", UserName = "a" };

			var jwt = new JwtDto { Token = "jwt-token", CreatedOn = DateTime.UtcNow, ExpiresOn = DateTime.UtcNow.AddMinutes(20) };
			var refresh = new RefreshTokenDto { Token = "rt-token", CreatedOn = DateTime.UtcNow, ExpiresOn = DateTime.UtcNow.AddDays(5) };

			_createTokenMock.Setup(c => c.CreateJwtToken(user, "Customer")).Returns(jwt);
			_createTokenMock.Setup(c => c.CreatRefreshToken()).Returns(refresh);
			_userServiceMock.Setup(s => s.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

			// Act
			var result = await _handler.CreateTokens(user, "Customer");

			// Assert
			ClassicAssert.IsNotNull(result);
			ClassicAssert.AreEqual(jwt.Token, result.Token);
			ClassicAssert.AreEqual(jwt.CreatedOn, result.CreatedOn);
			ClassicAssert.AreEqual(jwt.ExpiresOn, result.ExpiresOn);

			// Cookie set
			var setCookie = _httpContext.Response.Headers["Set-Cookie"].ToString();
			StringAssert.Contains("RefreshToken=", setCookie);
			StringAssert.Contains("rt-token", setCookie);

			// User refresh tokens updated
			ClassicAssert.IsNotEmpty(user.lkpRefreshTokens);
			var added = user.lkpRefreshTokens.FirstOrDefault(rt => rt.Token == refresh.Token);
			ClassicAssert.IsNotNull(added);
			ClassicAssert.AreEqual(refresh.CreatedOn, added.CreatedAt);
			ClassicAssert.AreEqual(refresh.ExpiresOn, added.ExpiresAt);

			_userServiceMock.Verify(s => s.UpdateAsync(user), Times.Once);
		}
	}
}