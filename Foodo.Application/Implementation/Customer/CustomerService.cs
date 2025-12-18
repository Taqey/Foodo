using Foodo.Application.Abstraction.Customer;
using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Customer;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Response;
using Foodo.Domain.Enums;
using Foodo.Domain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Application.Implementation.Customer
{
	public class CustomerService : ICustomerService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUserService _userService;
		private readonly ICacheService _cacheService;

		public CustomerService(IUnitOfWork unitOfWork, IUserService userService, ICacheService cacheService)
		{
			_unitOfWork = unitOfWork;
			_userService = userService;
			_cacheService = cacheService;
		}



		#region Shops

		public async Task<ApiResponse<PaginationDto<ShopDto>>> ReadAllShops(ProductPaginationInput input)
		{
			string cacheKey = $"customer_merchant:list:all:{input.Page}:{input.PageSize}";
			var cached = _cacheService.Get<PaginationDto<ShopDto>>(cacheKey);
			if (cached != null) return ApiResponse<PaginationDto<ShopDto>>.Success(cached);

			var query = _unitOfWork.RestaurantCustomRepository.ReadRestaurants();
			var totalCount = await query.CountAsync();
			if (totalCount == 0)
			{
				var emptyResult = new PaginationDto<ShopDto>
				{
					TotalItems = 0,
					TotalPages = 0,
					Items = new List<ShopDto>()
				};
				_cacheService.Set(cacheKey, emptyResult);
				return ApiResponse<PaginationDto<ShopDto>>.Success(emptyResult, "Restaurants not found");
			}
			var totalPages = (int)Math.Ceiling(totalCount / (double)input.PageSize);
			var shopDtos = await query
				.Select(e => new ShopDto
				{
					ShopId = e.UserId,
					ShopName = e.StoreName,
					ShopDescription = e.StoreDescription,
					Categories = e.TblRestaurantCategories
					.Select(c => ((RestaurantCategory)c.categoryid).ToString())
					.ToList(),
					url = e.User.UserPhoto.Url ?? null
				})
				.Skip((input.Page - 1) * input.PageSize)
				.Take(input.PageSize)
				.ToListAsync();

			var resultDto = new PaginationDto<ShopDto>
			{
				TotalItems = totalCount,
				TotalPages = totalPages,
				Items = shopDtos
			};

			_cacheService.Set(cacheKey, resultDto);
			return ApiResponse<PaginationDto<ShopDto>>.Success(resultDto);
		}

		public async Task<ApiResponse<ShopDto>> ReadShopById(ItemByIdInput input)
		{

			var query = _unitOfWork.RestaurantCustomRepository.ReadRestaurants();
			var filteredQuery = query.Where(e => e.UserId == input.ItemId);
			var shopDto = await filteredQuery.Select(e => new ShopDto
			{
				ShopId = e.UserId,
				ShopName = e.StoreName,
				ShopDescription = e.StoreDescription,
				Categories = e.TblRestaurantCategories
					.Select(c => ((RestaurantCategory)c.categoryid).ToString())
					.ToList(),
				url = e.User.UserPhoto.Url ?? null
			}).FirstOrDefaultAsync();

			if (shopDto == null) return ApiResponse<ShopDto>.Failure("Shop not found");


			return ApiResponse<ShopDto>.Success(shopDto, "Shop retrieved successfuly");
		}

		public async Task<ApiResponse<PaginationDto<ShopDto>>> ReadShopsByCategory(ShopsPaginationByCategoryInput input)
		{
			string cacheKey = $"customer_merchant:list:category:{input.Category}:{input.Page}:{input.PageSize}";
			var cached = _cacheService.Get<PaginationDto<ShopDto>>(cacheKey);
			if (cached != null) return ApiResponse<PaginationDto<ShopDto>>.Success(cached);
			var query = _unitOfWork.RestaurantCustomRepository.ReadRestaurants();
			var filteredQuery = query.Where(e => e.TblRestaurantCategories.Any(c => c.categoryid == (int)input.Category));
			var totalCount = await filteredQuery.CountAsync();
			if (totalCount == 0)
			{
				var emptyResult = new PaginationDto<ShopDto>
				{
					TotalItems = 0,
					TotalPages = 0,
					Items = new List<ShopDto>()
				};
				_cacheService.Set(cacheKey, emptyResult);
				return ApiResponse<PaginationDto<ShopDto>>.Success(emptyResult);
			}
			var totalPages = (int)Math.Ceiling(totalCount / (double)input.PageSize);
			var shopDtos = await filteredQuery
				.Select(e => new ShopDto
				{
					ShopId = e.UserId,
					ShopName = e.StoreName,
					ShopDescription = e.StoreDescription,
					Categories = e.TblRestaurantCategories
					.Select(c => ((RestaurantCategory)c.categoryid).ToString())
					.ToList(),
					url = e.User.UserPhoto.Url ?? null
				})
				.Skip((input.Page - 1) * input.PageSize)
				.Take(input.PageSize)
				.ToListAsync();
			//var (shops, totalCount, totalPages) = await _unitOfWork.MerchantRepository.PaginationAsync(input.Page, input.PageSize, m => m.TblRestaurantCategories.Any(c => c.categoryid == (int)input.Category));



			//		var shopDtos = shops.Select(shop => new ShopDto
			//		{
			//			ShopId = shop.UserId,
			//			ShopName = shop.StoreName,
			//			ShopDescription = shop.StoreDescription,
			//			Categories = shop.TblRestaurantCategories
			//.Select(c => ((RestaurantCategory)c.categoryid).ToString())
			//.ToList(),
			//			url = shop.User?.UserPhoto?.Url ?? null

			//		}).ToList();

			var resultDto = new PaginationDto<ShopDto>
			{
				TotalItems = totalCount,
				TotalPages = totalPages,
				Items = shopDtos
			};

			_cacheService.Set(cacheKey, resultDto);
			return ApiResponse<PaginationDto<ShopDto>>.Success(resultDto);
		}


		#endregion
	}
}
