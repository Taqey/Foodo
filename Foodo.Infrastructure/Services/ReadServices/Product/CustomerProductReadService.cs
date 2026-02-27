using Dapper;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Product;
using Foodo.Application.Models.Dto;
using Foodo.Application.Models.Dto.Photo;
using Foodo.Application.Models.Dto.Product;
using System.Data;

namespace Foodo.Infrastructure.Services.ReadServices.Product
{
	public class CustomerProductReadService : ICustomerProductReadService
	{
		private readonly IDbConnection _connection;

		public CustomerProductReadService(IDbConnection connection)
		{
			_connection = connection;
		}
		public async Task<CustomerProductDto> ReadProduct(int productId)
		{
			string query = @"select * from vw_CustomerProduct
                  where ProductId=@ProductId
";
			var RawProductDto = await _connection.QueryAsync<MerchantRawProductDto>(query, new { ProductId = productId });
			if (RawProductDto.Count() == 0)
			{
				return null;
			}
			var result = RawProductDto.GroupBy(o => o.ProductId).Select(e => new CustomerProductDto
			{
				ProductId = e.First().ProductId,
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
				ProductCategories = e.SelectMany(o => new List<string> { o.CategoryName }).Distinct().ToList(),
				Attributes = e
							.GroupBy(a => new { a.AttributeName, a.AttributeValue, a.MeasuringUnit })
							.Select(g => new AttributeDto
							{
								Name = g.Key.AttributeName,
								Value = g.Key.AttributeValue,
								MeasurementUnit = g.Key.MeasuringUnit
							})
							.ToList()
			}).FirstOrDefault();
			return result;
		}
	}
}
