using Foodo.API.Models.Request.Photo;
using Foodo.Application.Commands.Photos.AddUserPhoto;
using Foodo.Application.Queries.Photos.ReadUserPhoto;
using Foodo.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Foodo.API.Controllers
{
	/// <summary>
	/// Provides endpoints to manage user photos.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This controller handles operations related to uploading and retrieving
	/// photos associated with user profiles.
	/// </para>
	/// <para>
	/// Endpoints are rate-limited to prevent abuse and require authenticated users
	/// to identify the target profile.
	/// </para>
	/// </remarks>
	[Route("api/[controller]")]
	[ApiController]
	[EnableRateLimiting("LeakyBucketPolicy")]

	public class PhotosController : ControllerBase
	{
		private readonly IMediator _mediator;

		public PhotosController(IMediator mediator)
		{
			_mediator = mediator;
		}
		/// <summary>
		/// Retrieves the profile photo of the authenticated user.
		/// </summary>
		/// <returns>User photo data.</returns>
		/// <response code="200">User photo retrieved successfully.</response>
		/// <response code="400">Failed to retrieve user photo.</response>
		[HttpGet]
		public async Task<IActionResult> GetUserPhoto()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var result = await _mediator.Send(new ReadUserPhotoQuery { Id = userId, });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}

			return Ok(result.Data);
		}
		/// <summary>
		/// Uploads or updates the profile photo of the authenticated user.
		/// </summary>
		/// <param name="request">Photo upload request.</param>
		/// <returns>Status message.</returns>
		/// <response code="200">User photo uploaded successfully.</response>
		/// <response code="400">Failed to upload user photo.</response>

		[HttpPost]
		public async Task<IActionResult> AddUserPhoto([FromForm] AddPhotoRequest request)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
			var Enumvalue = (UserType)Enum.Parse(typeof(UserType), userRole);
			var result = await _mediator.Send(new AddUserPhotoCommand { file = request.file, Id = userId, UserType = Enumvalue });
			if (!result.IsSuccess)
			{
				return BadRequest(result.Message);
			}

			return Ok(result.Message);
		}

	}
}
