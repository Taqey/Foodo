using Foodo.Application.Abstraction.InfrastructureRelatedServices.User;
using Foodo.Application.Models.Response;
using Foodo.Domain.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Application.Commands.Addresses.SetAddressDefault
{
	public class SetAddressDefaultCommandHandler : IRequestHandler<SetAddressDefaultCommand, ApiResponse>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUserService _userService;

		public SetAddressDefaultCommandHandler(IUnitOfWork unitOfWork, IUserService userService)
		{
			_unitOfWork = unitOfWork;
			_userService = userService;
		}
		public async Task<ApiResponse> Handle(SetAddressDefaultCommand input, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.UserCustomRepository.ReadCustomer().Where(e => e.Id == input.CustomerId).FirstOrDefaultAsync();
			if (user == null)
			{
				return new ApiResponse { IsSuccess = false, Message = "user not found" };
			}
			var adresses = user.TblAdresses;
			if (adresses != null && adresses.Any())
			{
				var defaultadress = adresses.FirstOrDefault(e => e.IsDefault == true);
				if (defaultadress != null)
				{
					defaultadress.IsDefault = false;

				}

			}
			var newdefaultadress = adresses.FirstOrDefault(e => e.AddressId == input.AdressId);
			newdefaultadress.IsDefault = true;
			var result = await _userService.UpdateAsync(user);
			if (result.Succeeded)
			{
				return new ApiResponse { IsSuccess = true, Message = "Adress set default successfully" };
			}
			return new ApiResponse { Message = "Adress setting to default failed", IsSuccess = false };
		}
	}
}
