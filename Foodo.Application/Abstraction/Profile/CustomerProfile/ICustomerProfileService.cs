using Foodo.Application.Models.Dto.Profile.Customer;
using Foodo.Application.Models.Input.Profile.Customer;
using Foodo.Application.Models.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Abstraction.Profile.CustomerProfile
{
	public interface ICustomerProfileService
	{
		Task<ApiResponse<CustomerProfileDto>> GetCustomerProfile(CustomerGetCustomerProfileInput input);
		Task<ApiResponse> AddAdress(CustomerAddAdressInput input);
		Task<ApiResponse> RemoveAdress(CustomerRemoveAdressInput input);
		Task<ApiResponse> MakeAdressDefault(CustomerMakeAdressDefaultInput input);
	}
}
