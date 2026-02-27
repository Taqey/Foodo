using Dapper;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.Cache;
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

		public CustomerProductsReadService(
			IDbConnection connection,
			ICacheService cacheService)
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
			string cacheKey =
				$"customer_product:list:{page}:{pageSize}:{restaurantId}:{categoryId}:{orderBy}:{direction}";

			var cached = _cacheService.Get<PaginationDto<CustomerProductDto>>(cacheKey);
			if (cached != null)
				return cached;

			#region Count

			var countSql = new StringBuilder(@"
                SELECT COUNT(DISTINCT p.ProductId)
                FROM TblProducts p
                JOIN TblProductCategories pc ON p.ProductId = pc.ProductId
                WHERE p.IsDeleted = 0
            ");

			var countParams = new DynamicParameters();

			if (categoryId.HasValue)
			{
				countSql.Append(" AND pc.CategoryId = @CategoryId");
				countParams.Add("CategoryId", categoryId);
			}

			if (!string.IsNullOrEmpty(restaurantId))
			{
				countSql.Append(" AND p.UserId = @UserId");
				countParams.Add("UserId", restaurantId);
			}

			int totalItems = await _connection.ExecuteScalarAsync<int>(
				countSql.ToString(),
				countParams
			);

			if (totalItems == 0)
			{
				return new PaginationDto<CustomerProductDto>
				{
					Items = new List<CustomerProductDto>(),
					TotalItems = 0,
					TotalPages = 0
				};
			}

			int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

			#endregion

			#region OrderBy

			string orderByColumn = orderBy switch
			{
				ProductOrderBy.Name => "ProductsName",
				ProductOrderBy.Price => "Price",
				_ => "ProductId"
			};

			string orderDirection =
				direction == OrderingDirection.Descending ? "DESC" : "ASC";

			#endregion

			#region Get ProductIds (Pagination)

			var sql = new StringBuilder(@"
                SELECT DISTINCT
                    p.ProductId,
                    p.ProductsName,
                    pd.Price
                FROM TblProducts p
                JOIN TblProductCategories pc ON p.ProductId = pc.ProductId
                JOIN TblProductDetails pd ON p.ProductId = pd.ProductId
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

			sql.Append($@"
                ORDER BY {orderByColumn} {orderDirection}
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY
            ");

			parameters.Add("Offset", (page - 1) * pageSize);
			parameters.Add("PageSize", pageSize);

			var productIds = (await _connection
					.QueryAsync<int>(sql.ToString(), parameters))
					.ToList();

			#endregion

			#region Get Product Details

			var detailsSql = $@"
                SELECT  
	* from vw_CustomerProducts
                WHERE ProductId IN @productIds
                ORDER BY {orderByColumn} {orderDirection}
            ";

			var rows = await _connection.QueryAsync<CustomerRawProductDto>(
				detailsSql,
				new { productIds }
			);

			#endregion

			#region Mapping

			var products = rows
				.GroupBy(x => x.ProductId)
				.Select(g => new CustomerProductDto
				{
					ProductId = g.Key,
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
						.GroupBy(a => new
						{
							a.AttributeName,
							a.AttributeValue,
							a.MeasuringUnit
						})
						.Select(a => new AttributeDto
						{
							Name = a.Key.AttributeName,
							Value = a.Key.AttributeValue,
							MeasurementUnit = a.Key.MeasuringUnit
						})
						.ToList()
				})
				.ToList();

			#endregion

			var result = new PaginationDto<CustomerProductDto>
			{
				Items = products,
				TotalItems = totalItems,
				TotalPages = totalPages
			};

			_cacheService.Set(cacheKey, result);
			return result;
		}
	}
}
