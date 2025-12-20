using Foodo.Application.Abstraction.InfrastructureRelatedServices.User;
using Foodo.Application.Models.Response;
using Foodo.Domain.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Application.Commands.Addresses.DeleteAddress.DeleteMerchantAddress
{
	public class DeleteMerchantAddressCommandHandler : IRequestHandler<DeleteMerchantAddressCommand, ApiResponse>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUserService _userService;

		public DeleteMerchantAddressCommandHandler(IUnitOfWork unitOfWork, IUserService userService)
		{
			_unitOfWork = unitOfWork;
			_userService = userService;
		}
		public async Task<ApiResponse> Handle(DeleteMerchantAddressCommand input, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.UserCustomRepository.ReadMerchants().Where(e => e.Id == input.userId).FirstOrDefaultAsync();
			if (user == null)
			{
				return new ApiResponse { IsSuccess = false, Message = "user not found" };
			}
			var result = user.TblAdresses.Remove(user.TblAdresses.FirstOrDefault(e => e.AddressId == input.adressId));

			if (!result)
			{
				return new ApiResponse { IsSuccess = false, Message = "adress not found" };
			}
			var updateresult = await _userService.UpdateAsync(user);
			if (updateresult.Succeeded)
			{

				return new ApiResponse { IsSuccess = true, Message = "Adress deleted successfully" };
			}
			return new ApiResponse { IsSuccess = false, Message = "Adress deletion Failed" };
		}
	}
}
