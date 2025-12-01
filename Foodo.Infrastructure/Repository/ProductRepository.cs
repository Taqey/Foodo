using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using Foodo.Infrastructure.Perisistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Infrastructure.Repository
{
	public class ProductRepository : Repository<TblProduct>, IProductRepository
	{
		private readonly AppDbContext _context;

		public ProductRepository(AppDbContext context) : base(context)
		{
			_context = context;
		}

		public  IQueryable<TblProduct> ReadProducts()
		{
			var query =  _context.TblProducts
				.AsNoTracking();
			return query;
		}
		public IQueryable<TblProduct> ReadProductsInclude()
		{
			var query = _context.TblProducts
				.Include(p => p.TblProductDetails)
					.ThenInclude(pd => pd.LkpProductDetailsAttributes)
						.ThenInclude(a => a.Attribute)
				.Include(p => p.TblProductDetails)
					.ThenInclude(pd => pd.LkpProductDetailsAttributes)
						.ThenInclude(a => a.UnitOfMeasure)
				.Include(p => p.ProductCategories)
					.ThenInclude(pc => pc.Category)
				.Include(p => p.ProductPhotos)
				.Include(p => p.Merchant)
				.AsNoTracking();
			return query;
		}
		public IQueryable<TblProduct> ReadProductsIncludeTracking()
		{
			var query = _context.TblProducts
				.Include(p => p.TblProductDetails)
					.ThenInclude(pd => pd.LkpProductDetailsAttributes)
						.ThenInclude(a => a.Attribute)
				.Include(p => p.TblProductDetails)
					.ThenInclude(pd => pd.LkpProductDetailsAttributes)
						.ThenInclude(a => a.UnitOfMeasure)
				.Include(p => p.ProductCategories)
					.ThenInclude(pc => pc.Category)
				.Include(p => p.ProductPhotos)
				.Include(p => p.Merchant);
			return query;
		}
	}
}
