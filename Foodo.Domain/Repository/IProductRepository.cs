using Foodo.Domain.Entities;

namespace Foodo.Domain.Repository
{
	public interface IProductRepository : IRepository<TblProduct>
	{
		IQueryable<TblProduct> ReadProducts();
		IQueryable<TblProduct> ReadProductsInclude();
		IQueryable<TblProduct> ReadProductsIncludeTracking();
	}
}
