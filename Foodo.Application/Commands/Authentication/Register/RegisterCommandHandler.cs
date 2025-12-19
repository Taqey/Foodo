using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Models.Dto.Auth;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Enums;
using Foodo.Domain.Repository;
using MediatR;

namespace Foodo.Application.Commands.Authentication.Register
{
	public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<UserIdDto>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUserService _userService;
		private readonly ICacheService _cacheService;

		public RegisterCommandHandler(IUnitOfWork unitOfWork, IUserService userService, ICacheService cacheService)
		{
			_unitOfWork = unitOfWork;
			_userService = userService;
			_cacheService = cacheService;
		}
		public async Task<ApiResponse<UserIdDto>> Handle(RegisterCommand input, CancellationToken cancellationToken)
		{
			await using var transaction = await _unitOfWork.BeginTransactionAsync();

			try
			{
				// 1) Check if email exists
				var existingUser = await _userService.GetByEmailAsync(input.Email);
				if (existingUser != null)
					return ApiResponse<UserIdDto>.Failure("Email is already in use.");

				// 2) Check if username exists
				existingUser = await _userService.GetByUsernameAsync(input.UserName);
				if (existingUser != null)
					return ApiResponse<UserIdDto>.Failure("Username is already taken.");

				// 3) Create base identity user
				var user = new ApplicationUser
				{
					Email = input.Email,
					UserName = input.UserName,
					PhoneNumber = input.PhoneNumber
				};

				var result = await _userService.CreateUserAsync(user, input.Password);
				if (!result.Succeeded)
				{
					return ApiResponse<UserIdDto>.Failure("User creation failed: "
						+ string.Join(", ", result.Errors.Select(e => e.Description)));
				}

				string role = "";

				// 4) Add Customer or Merchant
				if (input.UserType == UserType.Customer)
				{
					user.TblCustomer = new TblCustomer
					{
						FirstName = input.FirstName,
						LastName = input.LastName,
						Gender = input.Gender.ToString(),
						BirthDate = (DateOnly)input.DateOfBirth
					};

					user.TblAdresses.Add(new TblAdress
					{
						City = input.City,
						State = input.State,
						PostalCode = input.PostalCode,
						Country = input.Country,
						IsDefault = true,
						StreetAddress = input.StreetAddress,
					});

					role = "Customer";
				}
				else
				{
					user.TblMerchant = new TblMerchant
					{
						StoreName = input.StoreName,
						StoreDescription = input.StoreDescription
					};

					role = "Merchant";
				}

				// 5) Save customer/merchant data
				await _unitOfWork.saveAsync();

				// 6) Add Role
				result = await _userService.AddRolesToUser(user, role);
				if (!result.Succeeded)
				{
					return ApiResponse<UserIdDto>.Failure("Failed to assign role: "
						+ string.Join(", ", result.Errors.Select(e => e.Description)));
				}

				// 7) Commit transaction
				await transaction.CommitAsync();

				// 8) Clear cache if merchant
				if (input.UserType == UserType.Merchant)
				{
					_cacheService.RemoveByPrefix("customer_merchant:list:");
				}

				return ApiResponse<UserIdDto>.Success(new UserIdDto { UserId = user.Id }, "User registered successfully.");
			}
			catch
			{
				// Rollback on error
				await transaction.RollbackAsync();
				return ApiResponse<UserIdDto>.Failure("User registration failed.");
			}
		}
	}
}
