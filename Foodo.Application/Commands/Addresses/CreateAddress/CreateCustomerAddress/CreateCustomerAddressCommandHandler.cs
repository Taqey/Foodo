using Autofac.Core;
using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Commands.Addresses.CreateAddress.CreateCustomerAddress
{
	public class CreateCustomerAddressCommandHandler : IRequestHandler<CreateCustomerAddressCommand, ApiResponse>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUserService _userService;

		public CreateCustomerAddressCommandHandler(IUnitOfWork unitOfWork,IUserService userService)
		{
			_unitOfWork = unitOfWork;
			_userService = userService;
		}
		public async Task<ApiResponse> Handle(CreateCustomerAddressCommand input, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.UserCustomRepository.ReadCustomer().Where(e => e.Id == input.UserId).FirstOrDefaultAsync();
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
