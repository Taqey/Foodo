using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;

namespace Foodo.Application.Abstraction.Merchant;

public interface IMerchantService
{

	Task<ApiResponse<PaginationDto<CustomerDto>>> ReadAllPurchasedCustomersAsync(ProductPaginationInput input);
}
