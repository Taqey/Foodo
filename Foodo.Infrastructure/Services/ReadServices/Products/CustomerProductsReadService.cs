using Dapper;
using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Products;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Photo;
using Foodo.Application.Models.Dto.Product;
using Foodo.Application.Models.Enums;
using Foodo.Domain.Enums;
using System.Data;
using System.Text;

namespace Foodo.Infrastructure.Services.ReadServices.Products
{
	public class CustomerProductsReadService : ICustomerProductsReadService
	{
		private readonly IDbConnection _connection;
		private readonly ICacheService _cacheService;

		public CustomerProductsReadService(IDbConnection connection, ICacheService cacheService)
		{
			_connection = connection;
			_cacheService = cacheService;
		}

		public async Task<PaginationDto<CustomerProductDto>> ReadProducts(
			int page,
			int pageSize,
			string? restaurantId,
			FoodCategory? categoryId,
			ProductOrderBy? orderBy,
			OrderingDirection? direction)
		{
			string cacheKey = $"customer_product:list:all:{page}:{pageSize}:{restaurantId}:{categoryId}:{orderBy}:{direction}";
			var cached = _cacheService.Get<PaginationDto<CustomerProductDto>>(cacheKey);
			if (cached != null) return cached;
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
			var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

			if (totalItems == 0)
			{
				return new PaginationDto<CustomerProductDto>
				{
					Items = new List<CustomerProductDto>(),
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

			parameters.Add("Offset", (page - 1) * pageSize);
			parameters.Add("PageSize", pageSize);
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
			var rows = await _connection.QueryAsync<CustomerRawProductDto>(prodQuery, new { productIds });
			var products = rows
							.GroupBy(o => o.ProductId)
							.Select(g => new CustomerProductDto
							{
								ProductId = g.First().ProductId,
								ProductName = g.First().ProductsName,
								ProductDescription = g.First().ProductDescription,
								Price = g.First().Price,
								Urls = g
									.GroupBy(p => p.Url)
									.Select(p => new ProductPhotosDto
									{
										url = p.Key,
										isMain = p.First().IsMain
									})
									.ToList(),
								ProductCategories = g
									.Select(x => x.CategoryName)
									.Distinct()
									.ToList(),
								Attributes = g
									.GroupBy(a => new { a.AttributeName, a.AttributeValue, a.MeasuringUnit })
									.Select(a => new AttributeDto
									{
										Name = a.Key.AttributeName,
										Value = a.Key.AttributeValue,
										MeasurementUnit = a.Key.MeasuringUnit
									})
									.ToList()
							})
							.ToList();

			_cacheService.Set(cacheKey, products);

			return new PaginationDto<CustomerProductDto>
			{
				Items = products,
				TotalItems = totalItems,
				TotalPages = totalPages
			};
		}
	}
}
