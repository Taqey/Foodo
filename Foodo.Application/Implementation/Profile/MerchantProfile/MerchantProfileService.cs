using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Abstraction.Profile.MerchantProfile;
using Foodo.Application.Models.Dto.Profile.Merchant;
using Foodo.Application.Models.Input.Profile.Merchant;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Enums;
using Foodo.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Implementation.Profile.MerchantProfile
{
	public class MerchantProfileService : IMerchantProfileService
	{
		private readonly IUserService _userService;
		private readonly IUnitOfWork _unitOfWork;

		public MerchantProfileService(IUserService userService,IUnitOfWork unitOfWork)
		{
			_userService = userService;
			_unitOfWork = unitOfWork;
		}
		public async Task<ApiResponse> AddAdress(MerchantAddAdressInput input)
		{
			var user =await _unitOfWork.UserCustomRepository.ReadMerchants().Where(e=>e.Id==input.MerchantId).FirstOrDefaultAsync();
			if (user == null) {
			return new ApiResponse { Message="user not found",IsSuccess=false};
			}
			foreach (var adress in input.Adresses)
			{
			user.TblAdresses.Add(new TblAdress { City=adress.City,PostalCode=adress.PostalCode,Country=adress.Country,State=adress.State,StreetAddress=adress.StreetAddress});
			}
			var result=await _userService.UpdateAsync(user);
			if (result.Succeeded)
			{
				return new ApiResponse { IsSuccess = true,Message= "Adress added successfully" };
			}
			return new ApiResponse { Message="Adress adding failed",IsSuccess= false }	;
		}

		public async Task<ApiResponse<MerchantProfileDto>> GetMerchantProfile(MerchantProfileInput input)
		{
			var user = await _unitOfWork.UserCustomRepository.ReadMerchants().Where(e => e.Id == input.MerchantId).FirstOrDefaultAsync();
			if (user == null) {
				return new ApiResponse<MerchantProfileDto> { IsSuccess = false, Message = "use not found" };
			}
			var MerchantProfile = new MerchantProfileDto { Email = user.Email, IsEmailConfirmed = user.EmailConfirmed, StoreName = user.TblMerchant.StoreName, StoreDescription = user.TblMerchant.StoreDescription };
			var addresses = user.TblAdresses.ToList();
			if (addresses != null && addresses.Any())
			{
			var AdressDto = new List<MerchantAdressDto>();
				foreach (var address in addresses)
				{
					AdressDto.Add(new MerchantAdressDto
					{
						Id = address.AddressId,
						StreetAddress = address.StreetAddress,
						City = address.City,
						Country = address.Country,
						PostalCode = address.PostalCode,
						State = address.State,
						IsDefault = address.IsDefault
					});
				}
				MerchantProfile.Adresses.AddRange(AdressDto);
			}
			var categories = user.TblMerchant.TblRestaurantCategories.ToList();

			if (categories != null && categories.Any())
			{
				foreach (var category in categories)
				{
					
						var enumValue = (RestaurantCategory)category.categoryid;

						MerchantProfile.categories.Add(enumValue.ToString());
					
				}
			}

			return new ApiResponse<MerchantProfileDto> { Data = MerchantProfile, IsSuccess = true, Message = "data retrieved successfully" };
		}

		public async Task<ApiResponse> RemoveAdress(MerchantRemoveAdressInput input)
		{
			var user = await _unitOfWork.UserCustomRepository.ReadMerchants().Where(e => e.Id == input.MerchantId).FirstOrDefaultAsync();
			if (user == null)
			{
				return new ApiResponse { IsSuccess = false, Message = "user not found" };
			}
			var result=user.TblAdresses.Remove(user.TblAdresses.FirstOrDefault(e => e.AddressId == input.AdressId));
			
			if (!result)
			{
				return new ApiResponse { IsSuccess = false, Message = "adress not found" };
			}
			var updateresult=await _userService.UpdateAsync(user);
			if (updateresult.Succeeded)
			{
				
			return new ApiResponse { IsSuccess = true, Message = "Adress deleted successfully" };
			}
			return new ApiResponse { IsSuccess = false, Message = "Adress deletion Failed" };

		}
	}
}
