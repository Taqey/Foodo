using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Abstraction.Product;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Photo;
using Foodo.Application.Models.Dto.Product;
using Foodo.Application.Models.Input;
using Foodo.Application.Models.Input.Customer;
using Foodo.Application.Models.Response;
using Foodo.Domain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Application.Implementation.Product
{
	public class CustomerProductService : ICustomerProductService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICacheService _cacheService;
		private readonly IUserService _userService;

		public CustomerProductService(IUnitOfWork unitOfWork, ICacheService cacheService, IUserService userService)
		{
			_unitOfWork = unitOfWork;
			_cacheService = cacheService;
			_userService = userService;
		}
		public async Task<ApiResponse<CustomerProductDto>> ReadProduct(ItemByIdInput input)
		{
			var query = _unitOfWork.ProductCustomRepository.ReadProductsInclude();
			var productDto = await query
		.Select(e => new CustomerProductDto
		{
			ProductId = e.ProductId,
			ProductName = e.ProductsName,
			ProductDescription = e.ProductDescription,
			Price = e.TblProductDetails
						.OrderBy(pd => pd.ProductId)
						.Select(pd => pd.Price.ToString())
						.FirstOrDefault() ?? "0",
			Attributes = e.TblProductDetails.SelectMany(p => p.LkpProductDetailsAttributes).Select(p => new AttributeDto
			{
				Name = p.Attribute.Name,
				Value = p.Attribute.value,
				MeasurementUnit = p.UnitOfMeasure.UnitOfMeasureName
			}).ToList(),
			Urls = e.ProductPhotos
						.Select(p => new ProductPhotosDto
						{
							url = p.Url,
							isMain = p.isMain
						}).ToList(),
			ProductCategories = e.ProductCategories
						.Select(pc => pc.Category.CategoryName)
						.ToList()
		})
		.FirstOrDefaultAsync(e => e.ProductId == Convert.ToInt32(input.ItemId));

			if (productDto == null) return ApiResponse<CustomerProductDto>.Failure("Product not found");



			return ApiResponse<CustomerProductDto>.Success(productDto);
		}

		public async Task<ApiResponse<PaginationDto<CustomerProductDto>>> ReadProducts(ProductPaginationInput input)
		{
			string cacheKey = $"customer_product:list:all:{input.Page}:{input.PageSize}";
			var cached = _cacheService.Get<PaginationDto<CustomerProductDto>>(cacheKey);
			if (cached != null) return ApiResponse<PaginationDto<CustomerProductDto>>.Success(cached);

			var query = _unitOfWork.ProductCustomRepository.ReadProducts();

			var totalCount = await query.CountAsync();
			if (totalCount == 0)
			{
				var emptyResult = new PaginationDto<CustomerProductDto>
				{
					TotalItems = 0,
					TotalPages = 0,
					Items = new List<CustomerProductDto>()
				};
				_cacheService.Set(cacheKey, emptyResult);
				return ApiResponse<PaginationDto<CustomerProductDto>>.Success(emptyResult);
			}
			var totalPages = (int)Math.Ceiling(totalCount / (double)input.PageSize);

			var productDtos = await query
				.Select(e => new CustomerProductDto
				{
					ProductId = e.ProductId,
					ProductName = e.ProductsName,
					ProductDescription = e.ProductDescription,
					Price = e.TblProductDetails
								.Select(pd => pd.Price.ToString())
								.FirstOrDefault() ?? "0",
					Attributes = e.TblProductDetails.SelectMany(p => p.LkpProductDetailsAttributes).Select(p => new AttributeDto
					{
						Name = p.Attribute.Name,
						Value = p.Attribute.value,
						MeasurementUnit = p.UnitOfMeasure.UnitOfMeasureName
					}).ToList(),
					Urls = e.ProductPhotos
								.Select(p => new ProductPhotosDto
								{
									url = p.Url,
									isMain = p.isMain
								}).ToList(),
					ProductCategories = e.ProductCategories
						.Select(pc => pc.Category.CategoryName)
						.ToList()
				})
				.Skip((input.Page - 1) * input.PageSize)
				.Take(input.PageSize)
				.ToListAsync();
			var resultDto = new PaginationDto<CustomerProductDto>
			{
				TotalItems = totalCount,
				TotalPages = totalPages,
				Items = productDtos
			};

			_cacheService.Set(cacheKey, resultDto);
			return ApiResponse<PaginationDto<CustomerProductDto>>.Success(resultDto);

		}

		public async Task<ApiResponse<PaginationDto<CustomerProductDto>>> ReadProductsByCategory(ProductPaginationByCategoryInput input)
		{
			string cacheKey = $"customer_product:list:category:{input.Category}:{input.Page}:{input.PageSize}";
			var cached = _cacheService.Get<PaginationDto<CustomerProductDto>>(cacheKey);
			if (cached != null) return ApiResponse<PaginationDto<CustomerProductDto>>.Success(cached);

			var query = _unitOfWork.ProductCustomRepository.ReadProductsInclude();
			var filteredQuery = query.Where(p => p.ProductCategories.Any(c => c.categoryid == (int)input.Category));
			var totalCount = await filteredQuery.CountAsync();
			if (totalCount == 0)
			{
				var emptyResult = new PaginationDto<CustomerProductDto>
				{
					TotalItems = 0,
					TotalPages = 0,
					Items = new List<CustomerProductDto>()
				};
				_cacheService.Set(cacheKey, emptyResult);
				return ApiResponse<PaginationDto<CustomerProductDto>>.Success(emptyResult);
			}
			var totalPages = (int)Math.Ceiling(totalCount / (decimal)input.PageSize);
			var productDtos = await filteredQuery
				.Select(e => new CustomerProductDto
				{
					ProductId = e.ProductId,
					ProductName = e.ProductsName,
					ProductDescription = e.ProductDescription,
					Price = (e.TblProductDetails.Single().Price).ToString(),
					Attributes = e.TblProductDetails.SelectMany(p => p.LkpProductDetailsAttributes).Select(p => new AttributeDto
					{
						Name = p.Attribute.Name,
						Value = p.Attribute.value,
						MeasurementUnit = p.UnitOfMeasure.UnitOfMeasureName
					}).ToList(),
					ProductCategories = e.ProductCategories.Select(c => c.Category.CategoryName).ToList(),
					Urls = e.ProductPhotos.Select(pp => new ProductPhotosDto
					{
						url = pp.Url,
						isMain = pp.isMain
					}).ToList()

				})
				.Skip((input.Page - 1) * input.PageSize)
				.Take(input.PageSize)
				.ToListAsync();

			var resultDto = new PaginationDto<CustomerProductDto>
			{
				TotalItems = totalCount,
				TotalPages = totalPages,
				Items = productDtos
			};

			_cacheService.Set(cacheKey, resultDto);
			return ApiResponse<PaginationDto<CustomerProductDto>>.Success(resultDto);
		}

		public async Task<ApiResponse<PaginationDto<CustomerProductDto>>> ReadProductsByShop(ProductPaginationByShopInput input)
		{
			string cacheKey = $"customer_product:list:shop:{input.MerchantId}:{input.Page}:{input.PageSize}";
			var user = await _userService.GetByIdAsync(input.MerchantId);
			if (user == null) return ApiResponse<PaginationDto<CustomerProductDto>>.Failure("Merchant not found");
			var query = _unitOfWork.ProductCustomRepository.ReadProducts();
			var filteredQuery = query.Where(e => e.UserId == input.MerchantId);
			var totalCount = await filteredQuery.CountAsync();
			if (totalCount == 0)
			{
				var emptyResult = new PaginationDto<CustomerProductDto>
				{
					TotalItems = 0,
					TotalPages = 0,
					Items = new List<CustomerProductDto>()
				};
				_cacheService.Set(cacheKey, emptyResult);
				return ApiResponse<PaginationDto<CustomerProductDto>>.Success(emptyResult);
			}

			var totalPages = (int)Math.Ceiling(totalCount / (decimal)input.PageSize);
			var productDtos = await filteredQuery
				.Select(e => new CustomerProductDto
				{
					ProductId = e.ProductId,
					ProductName = e.ProductsName,
					ProductDescription = e.ProductDescription,
					Price = (e.TblProductDetails.Single().Price).ToString(),
					Attributes = e.TblProductDetails.SelectMany(p => p.LkpProductDetailsAttributes).Select(p => new AttributeDto
					{
						Name = p.Attribute.Name,
						Value = p.Attribute.value,
						MeasurementUnit = p.UnitOfMeasure.UnitOfMeasureName
					}).ToList(),
					ProductCategories = e.ProductCategories.Select(c => c.Category.CategoryName).ToList(),
					Urls = e.ProductPhotos.Select(pp => new ProductPhotosDto
					{
						url = pp.Url,
						isMain = pp.isMain
					}).ToList()

				})
				.Skip((input.Page - 1) * input.PageSize)
				.Take(input.PageSize)
				.ToListAsync();


			var resultDto = new PaginationDto<CustomerProductDto>
			{
				TotalItems = totalCount,
				TotalPages = totalPages,
				Items = productDtos
			};
			_cacheService.Set(cacheKey, resultDto);
			return ApiResponse<PaginationDto<CustomerProductDto>>.Success(resultDto);
		}
	}
}
