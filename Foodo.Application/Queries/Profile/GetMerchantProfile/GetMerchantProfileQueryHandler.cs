using Foodo.Application.Models.Dto.Profile.Merchant;
using Foodo.Application.Models.Response;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Application.Queries.Profile.GetMerchantProfile
{
	public class GetMerchantProfileQueryHandler : IRequestHandler<GetMerchantProfileQuery, ApiResponse<MerchantProfileDto>>
	{
		public Task<ApiResponse<MerchantProfileDto>> Handle(GetMerchantProfileQuery request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
