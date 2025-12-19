using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Commands.Addresses.CreateAddress.CreateMerchantAddress
{
	public class CreateMerchantAddressCommandHandler : IRequestHandler<CreateMerchantAddressCommand, ApiResponse>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUserService _userService;

		public CreateMerchantAddressCommandHandler(IUnitOfWork unitOfWork,IUserService userService)
		{
			_unitOfWork = unitOfWork;
			_userService = userService;
		}


		public async Task<ApiResponse> Handle(CreateMerchantAddressCommand input, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.UserCustomRepository.ReadMerchants().Where(e => e.Id == input.UserId).FirstOrDefaultAsync();
			if (user == null)
			{
				return new ApiResponse { Message = "user not found", IsSuccess = false };
			}
			foreach (var adress in input.Adresses)
			{
				user.TblAdresses.Add(new TblAdress { City = adress.City, PostalCode = adress.PostalCode, Country = adress.Country, State = adress.State, StreetAddress = adress.StreetAddress });
			}
			var result = await _userService.UpdateAsync(user);
			if (result.Succeeded)
			{
				return new ApiResponse { IsSuccess = true, Message = "Adress added successfully" };
			}
			return new ApiResponse { Message = "Adress adding failed", IsSuccess = false };
		}
	}
}
