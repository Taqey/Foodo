using Dapper;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Product;
using Foodo.Application.Models.Dto.Merchant;
using Foodo.Application.Models.Dto.Photo;
using Foodo.Application.Models.Dto.Product;
using System.Data;

namespace Foodo.Infrastructure.Services.ReadServices.Product
{
	public class MerchantProductReadService : IMerchantProductReadService
	{
		private readonly IDbConnection _connection;

		public MerchantProductReadService(IDbConnection connection)
		{
			_connection = connection;
		}
		public async Task<MerchantProductDto> ReadProduct(int productId)
		{
			string query = @"SELECT  TblProducts.ProductId, TblProducts.ProductsName, TblProducts.ProductDescription, TblProductDetails.Price, TblProductPhotos.Url, TblProductPhotos.isMain, TblCategoryOfProducts.CategoryName, 
                   LkpProductDetailsAttributes.ProductDetailAttributeId,LkpAttributes.Name as 'AttributeName',LkpAttributes.value as 'AttributeValue',  LkpMeasureUnits.UnitOfMeasureName as 'MeasuringUnit'
FROM     TblCategoryOfProducts INNER JOIN
                  TblProductCategories ON TblCategoryOfProducts.CategoryId = TblProductCategories.categoryid INNER JOIN
                  TblProducts ON TblProductCategories.productid = TblProducts.ProductId INNER JOIN
                  TblProductDetails ON TblProducts.ProductId = TblProductDetails.ProductId INNER JOIN
                  TblProductPhotos ON TblProducts.ProductId = TblProductPhotos.ProductId INNER JOIN
                  LkpProductDetailsAttributes ON TblProductDetails.ProductDetailId = LkpProductDetailsAttributes.ProductDetailId INNER JOIN
                  LkpAttributes ON LkpProductDetailsAttributes.AttributeId = LkpAttributes.AttributeId INNER JOIN
                  LkpMeasureUnits ON LkpProductDetailsAttributes.UnitOfMeasureId = LkpMeasureUnits.UnitOfMeasureId
                  where TblProducts.ProductId=@ProductId
";
			var RawProductDto=await _connection.QueryAsync<MerchantRawProductDto>(query,new {ProductId= productId });
			if (RawProductDto.Count() == 0)
			{
				return null;
			}
			var result = RawProductDto.GroupBy(o => o.ProductId).Select(e=>new MerchantProductDto
			{
				ProductId=e.First().ProductId,
				ProductName=e.First().ProductsName,
				ProductDescription=e.First().ProductDescription,
				Price=e.First().Price,
				Urls=e.Select(o=>new ProductPhotosDto { isMain=o.IsMain,url=o.Url}).ToList(),
				ProductCategories=e.SelectMany(o=>new List<string> { o.CategoryName}).ToList(),
				ProductDetailAttributes=e.Select(o=>new ProductDetailAttributeDto { Id=o.ProductDetailAttributeId,AttributeName=o.AttributeName,AttributeValue=o.AttributeValue,MeasurementUnit=o.MeasuringUnit }).ToList()
				
			}).FirstOrDefault();
			return result;
		}
	}
}
