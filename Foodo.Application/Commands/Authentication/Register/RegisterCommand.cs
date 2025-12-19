using Foodo.Application.Models.Dto.Auth;
using Foodo.Application.Models.Response;
using Foodo.Domain.Enums;
using MediatR;

namespace Foodo.Application.Commands.Authentication.Register
{
	public class RegisterCommand : IRequest<ApiResponse<UserIdDto>>
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string PhoneNumber { get; set; }
		public Gender? Gender { get; set; }
		public DateOnly? DateOfBirth { get; set; }
		public string UserName { get; set; }
		public UserType UserType { get; set; } = UserType.Customer;
		public string? StoreName { get; set; }

		public string? StoreDescription { get; set; }
		public string? City { get; set; }
		public string? State { get; set; }
		public string? StreetAddress { get; set; }
		public string? PostalCode { get; set; }
		public string? Country { get; set; }
	}
}
