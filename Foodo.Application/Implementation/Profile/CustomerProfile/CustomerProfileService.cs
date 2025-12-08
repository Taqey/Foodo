using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Abstraction.Profile.CustomerProfile;
using Foodo.Application.Models.Dto.Profile.Customer;
using Foodo.Application.Models.Input.Profile.Customer;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Application.Implementation.Profile.CustomerProfile
{
	public class CustomerProfileService : ICustomerProfileService
	{
		private readonly IUserService _service;
		private readonly IUnitOfWork _unitOfWork;

		public CustomerProfileService(IUserService service, IUnitOfWork unitOfWork)
		{
			_service = service;
			_unitOfWork = unitOfWork;
		}
		public async Task<ApiResponse> AddAdress(CustomerAddAdressInput input)
		{
			var user = await _unitOfWork.UserCustomRepository.ReadCustomer().Where(e => e.Id == input.CustomerId).FirstOrDefaultAsync();
			if (user == null)
			{
				return new ApiResponse { Message = "user not found", IsSuccess = false };
			}
			foreach (var adress in input.Adresses)
			{
				user.TblAdresses.Add(new TblAdress { City = adress.City, PostalCode = adress.PostalCode, Country = adress.Country, State = adress.State, StreetAddress = adress.StreetAddress });
			}
			var result = await _service.UpdateAsync(user);
			if (result.Succeeded)
			{
				return new ApiResponse { IsSuccess = true, Message = "Adress added successfully" };
			}
			return new ApiResponse { Message = "Adress adding failed", IsSuccess = false };
		}

		public async Task<ApiResponse<CustomerProfileDto>> GetCustomerProfile(CustomerGetCustomerProfileInput input)
		{
			var user = await _unitOfWork.UserCustomRepository.ReadCustomer().Where(e => e.Id == input.UserId).FirstOrDefaultAsync();
			if (user == null)
			{
				return new ApiResponse<CustomerProfileDto> { IsSuccess = false, Message = "user not found" };
			}
			var Customer = user.TblCustomer;
			var CustomerDto = new CustomerProfileDto { FirstName = Customer.FirstName, LastName = Customer.LastName, BirthDate = Customer.BirthDate, Email = user.Email, IsEmailConfirmed = user.EmailConfirmed, Gender = Customer.Gender, PhoneNumber = user.PhoneNumber };
			var adresses = user.TblAdresses;
			if (adresses != null && adresses.Any())
			{
				foreach (var adress in adresses)
				{
					CustomerDto.Adresses.Add(new CustomerAdressDto { City = adress.City, Country = adress.Country, PostalCode = adress.PostalCode, IsDefault = adress.IsDefault, Id = adress.AddressId, State = adress.State, StreetAddress = adress.StreetAddress });
				}
			}
			return new ApiResponse<CustomerProfileDto> { Data = CustomerDto, Message = "Data retrieved successfully", IsSuccess = true };
		}

		public async Task<ApiResponse> MakeAdressDefault(CustomerMakeAdressDefaultInput input)
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
			var result = await _service.UpdateAsync(user);
			if (result.Succeeded)
			{
				return new ApiResponse { IsSuccess = true, Message = "Adress set default successfully" };
			}
			return new ApiResponse { Message = "Adress setting to default failed", IsSuccess = false };
		}

		public async Task<ApiResponse> RemoveAdress(CustomerRemoveAdressInput input)
		{
			var user = await _unitOfWork.UserCustomRepository.ReadCustomer().Where(e => e.Id == input.CustomerId).FirstOrDefaultAsync();
			if (user == null)
			{
				return new ApiResponse { IsSuccess = false, Message = "user not found" };
			}
			var result = user.TblAdresses.Remove(user.TblAdresses.FirstOrDefault(e => e.AddressId == input.adressId));

			if (!result)
			{
				return new ApiResponse { IsSuccess = false, Message = "adress not found" };
			}
			var updateresult = await _service.UpdateAsync(user);
			if (updateresult.Succeeded)
			{

				return new ApiResponse { IsSuccess = true, Message = "Adress deleted successfully" };
			}
			return new ApiResponse { IsSuccess = false, Message = "Adress deletion Failed" };
		}
	}
}
