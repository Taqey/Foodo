using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Abstraction.Merchant;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Response;
using Foodo.Domain.Enums;
using Foodo.Domain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Application.Implementation.Merchant;

public class MerchantService : IMerchantService
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IUserService _service;
	private readonly ICacheService _cacheService;

	public MerchantService(IUnitOfWork unitOfWork, IUserService service, ICacheService cacheService)
	{
		_unitOfWork = unitOfWork;
		_service = service;
		_cacheService = cacheService;
	}


	#region Customers

	public async Task<ApiResponse<PaginationDto<CustomerDto>>> ReadAllPurchasedCustomersAsync(ProductPaginationInput input)
	{
		var cacheKey = $"merchant_customer:list:{input.UserId}:{input.Page}:{input.PageSize}";

		// جلب من الكاش أولاً
		if (_cacheService.Get<PaginationDto<CustomerDto>>(cacheKey) is PaginationDto<CustomerDto> cachedResult)
			return ApiResponse<PaginationDto<CustomerDto>>.Success(cachedResult);

		var ordersQuery = _unitOfWork.OrderCustomRepository
			.ReadOrdersInclude()
			.Where(e => e.MerchantId == input.UserId);

		// لو مفيش أوردرز
		var totalCount = await ordersQuery.CountAsync();
		if (totalCount == 0)
		{
			var emptyResult = new PaginationDto<CustomerDto>
			{
				TotalItems = 0,
				TotalPages = 0,
				Items = new List<CustomerDto>()
			};
			_cacheService.Set(cacheKey, emptyResult);
			return ApiResponse<PaginationDto<CustomerDto>>.Success(emptyResult);
		}

		var totalPages = (int)Math.Ceiling(totalCount / (double)input.PageSize);

		// pagination للأوردرز
		var customerIds = await ordersQuery
			.Select(o => o.CustomerId)
			.Distinct()
			.Skip((input.Page - 1) * input.PageSize)
			.Take(input.PageSize)
			.ToListAsync();

		// جلب العملاء، لو مفيش يوزر مطابق، نرجع فاضي
		var customers = await _unitOfWork.UserCustomRepository
			.ReadCustomer()
			.Where(c => customerIds.Contains(c.Id))
			.ToListAsync();

		if (!customers.Any())
		{
			var emptyResult = new PaginationDto<CustomerDto>
			{
				TotalItems = 0,
				TotalPages = 0,
				Items = new List<CustomerDto>()
			};
			_cacheService.Set(cacheKey, emptyResult);
			return ApiResponse<PaginationDto<CustomerDto>>.Success(emptyResult);
		}

		// تجميع الأوردرز حسب العميل
		var groupedOrders = ordersQuery
			.Where(o => customerIds.Contains(o.CustomerId))
			.ToList()
			.GroupBy(o => o.CustomerId)
			.ToDictionary(g => g.Key, g => g.ToList());

		var customerDtos = new List<CustomerDto>();
		foreach (var c in customers)
		{
			// لو مفيش أوردرز لهذا العميل، نعديه
			if (!groupedOrders.TryGetValue(c.Id, out var custOrders) || custOrders == null || !custOrders.Any())
				continue;

			var completedOrders = custOrders.Where(o => o.OrderStatus == OrderState.Completed).ToList();

			customerDtos.Add(new CustomerDto
			{
				FullName = $"{c.TblCustomer?.FirstName ?? "Unknown"} {c.TblCustomer?.LastName ?? ""}",
				Email = c.Email ?? "N/A",
				PhoneNumber = c.PhoneNumber ?? "N/A",
				LastPurchased = completedOrders.Any() ? completedOrders.Max(o => o.OrderDate) : (DateTime?)null,
				TotalOrders = custOrders.Count,
				TotalSpent = completedOrders.Sum(o => Convert.ToDecimal(o.TotalPrice))
			});
		}

		var paginationDto = new PaginationDto<CustomerDto>
		{
			TotalItems = totalCount,
			TotalPages = totalPages,
			Items = customerDtos
		};

		_cacheService.Set(cacheKey, paginationDto);

		return ApiResponse<PaginationDto<CustomerDto>>.Success(paginationDto);
	}


	#endregion
}
