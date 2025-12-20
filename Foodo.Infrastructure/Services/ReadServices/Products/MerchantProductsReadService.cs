using Dapper;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.Cache;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Products;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Merchant;
using Foodo.Application.Models.Dto.Photo;
using Foodo.Application.Models.Dto.Product;
using Foodo.Application.Models.Enums;
using Foodo.Domain.Enums;
using System.Data;
using System.Text;

namespace Foodo.Infrastructure.Services.ReadServices.Products
{
	public class MerchantProductsReadService : IMerchantProductsReadService
	{
		private readonly IDbConnection _connection;
		private readonly ICacheService _cacheService;

		public MerchantProductsReadService(IDbConnection connection, ICacheService cacheService)
		{
			_connection = connection;
			_cacheService = cacheService;
		}

		public async Task<PaginationDto<MerchantProductDto>> ReadProducts(
			int Page,
			int PageSize,
			string? restaurantId,
			FoodCategory? categoryId,
			ProductOrderBy? orderBy,
			OrderingDirection? direction)
		{

			string cacheKey = $"merchant_product:list:{restaurantId}:{Page}:{PageSize}:{categoryId}:{orderBy}:{direction}";
			var cached = _cacheService.Get<PaginationDto<MerchantProductDto>>(cacheKey);
			if (cached != null)
			{
				return cached;
			}
			var countSql = new StringBuilder(@"
											SELECT COUNT(DISTINCT p.ProductId)
											FROM TblProducts p
											JOIN TblProductCategories pc ON p.ProductId = pc.ProductId
											WHERE p.IsDeleted = 0
											");

			var countParameters = new DynamicParameters();

			if (categoryId.HasValue)
			{
				countSql.Append(" AND pc.CategoryId = @CategoryId");
				countParameters.Add("CategoryId", categoryId);
			}

			if (!string.IsNullOrEmpty(restaurantId))
			{
				countSql.Append(" AND p.UserId = @UserId");
				countParameters.Add("UserId", restaurantId);
			}

			var totalItems = await _connection.ExecuteScalarAsync<int>(
	countSql.ToString(),
	countParameters
);
			var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

			if (totalItems == 0)
			{
				return new PaginationDto<MerchantProductDto>
				{
					Items = new List<MerchantProductDto>(),
					TotalItems = 0,
					TotalPages = 0
				};
			}

			var sql = new StringBuilder(@"
											SELECT DISTINCT p.ProductId
											FROM TblProducts p
											JOIN TblProductCategories pc ON p.ProductId = pc.ProductId
											WHERE p.IsDeleted = 0
											");

			var parameters = new DynamicParameters();
			if (categoryId.HasValue)
			{
				sql.Append(" AND pc.CategoryId = @CategoryId");
				parameters.Add("CategoryId", categoryId);
			}

			if (!string.IsNullOrEmpty(restaurantId))
			{
				sql.Append(" AND p.UserId = @UserId");
				parameters.Add("UserId", restaurantId);
			}
			string orderByColumn = "";
			switch (orderBy)
			{
				case ProductOrderBy.Name:
					orderByColumn = "p.ProductsName";
					break;

				case ProductOrderBy.Price:
					orderByColumn = "p.Price";
					break;

				default:
					orderByColumn = "p.ProductId";
					break;
			}

			sql.Append($" ORDER BY {orderByColumn}");
			sql.Append(@"
						 OFFSET @Offset ROWS
						 FETCH NEXT @PageSize ROWS ONLY
						");

			parameters.Add("Offset", (Page - 1) * PageSize);
			parameters.Add("PageSize", PageSize);
			var productIds = (await _connection.QueryAsync<int>(sql.ToString(), parameters)).ToList();

			var prodQuery = $@"SELECT  
					p.ProductId,
                    p.ProductsName,
                    p.ProductDescription,
                    TblProductDetails.Price,
                    TblProductPhotos.Url,
                    TblProductPhotos.isMain,
                    TblCategoryOfProducts.CategoryName,
                    LkpAttributes.Name AS AttributeName,
                    LkpAttributes.value AS AttributeValue,
                    LkpProductDetailsAttributes.ProductDetailAttributeId As ProductDetailAttributeId,
                    LkpMeasureUnits.UnitOfMeasureName AS MeasuringUnit,
                    p.CreatedDate
					FROM TblCategoryOfProducts
					INNER JOIN TblProductCategories ON TblCategoryOfProducts.CategoryId = TblProductCategories.categoryid
					INNER JOIN TblProducts p ON TblProductCategories.productid = p.ProductId
					INNER JOIN TblProductDetails ON p.ProductId = TblProductDetails.ProductId
					INNER JOIN TblProductPhotos ON p.ProductId = TblProductPhotos.ProductId
					INNER JOIN LkpProductDetailsAttributes ON TblProductDetails.ProductDetailId = LkpProductDetailsAttributes.ProductDetailId
					INNER JOIN LkpAttributes ON LkpProductDetailsAttributes.AttributeId = LkpAttributes.AttributeId
					INNER JOIN LkpMeasureUnits ON LkpProductDetailsAttributes.UnitOfMeasureId = LkpMeasureUnits.UnitOfMeasureId
					WHERE p.ProductId IN @productIds
					ORDER BY {orderByColumn}";

			var rows = await _connection.QueryAsync<MerchantRawProductDto>(
				prodQuery,
				new { productIds = productIds }
			);

			var products = rows
				.GroupBy(o => o.ProductId)
				.Select(e => new MerchantProductDto
				{
					ProductId = e.Key,
					ProductName = e.First().ProductsName,
					ProductDescription = e.First().ProductDescription,
					Price = e.First().Price,
					Urls = e
						.GroupBy(p => p.Url)
						.Select(g => new ProductPhotosDto
						{
							url = g.Key,
							isMain = g.First().IsMain
						})
						.ToList(),
					ProductCategories = e
						.Select(o => o.CategoryName)
						.Distinct()
						.ToList(),
					ProductDetailAttributes = e
						.GroupBy(a => new { a.AttributeName, a.AttributeValue, a.MeasuringUnit })
						.Select(g => new ProductDetailAttributeDto
						{
							Id = g.First().ProductDetailAttributeId,
							AttributeName = g.Key.AttributeName,
							AttributeValue = g.Key.AttributeValue,
							MeasurementUnit = g.Key.MeasuringUnit
						})
						.ToList()
				})
				.ToList();
			_cacheService.Set(cacheKey, products);

			return new PaginationDto<MerchantProductDto>
			{
				Items = products,
				TotalItems = totalItems,
				TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize)
			};
		}
	}
}
