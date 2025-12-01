using Foodo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Domain.Repository
{
	public interface IProductRepository :IRepository<TblProduct>
	{
		IQueryable<TblProduct> ReadProducts();
		IQueryable<TblProduct> ReadProductsInclude();
		IQueryable<TblProduct> ReadProductsIncludeTracking();
	}
}
