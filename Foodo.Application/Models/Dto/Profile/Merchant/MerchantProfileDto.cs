using Foodo.Domain.Entities;
using Foodo.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Models.Dto.Profile.Merchant
{
	public class MerchantProfileDto
	{
		public string StoreName { get; set; }
		public string StoreDescription { get; set; }
		public string Email { get; set; }
		public bool IsEmailConfirmed { get; set; }
		public List<MerchantAdressDto>? Adresses { get; set; }=new List<MerchantAdressDto>();
		public List <string>? categories { get; set; } = new List<string>();




	}
}
