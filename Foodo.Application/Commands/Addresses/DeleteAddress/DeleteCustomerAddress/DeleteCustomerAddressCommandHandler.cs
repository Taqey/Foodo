using Autofac.Core;
using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Models.Response;
using Foodo.Domain.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Commands.Addresses.DeleteAddress.DeleteCustomerAddress
{
	public class DeleteCustomerAddressCommandHandler : IRequestHandler<DeleteCustomerAddressCommand, ApiResponse>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUserService _userService;

		public DeleteCustomerAddressCommandHandler(IUnitOfWork unitOfWork,IUserService userService)
		{
			_unitOfWork = unitOfWork;
			_userService = userService;
		}
		public async Task<ApiResponse> Handle(DeleteCustomerAddressCommand input, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.UserCustomRepository.ReadCustomer().Where(e => e.Id == input.userId).FirstOrDefaultAsync();
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
