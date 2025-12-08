using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using Foodo.Infrastructure.Perisistence;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Infrastructure.Repository
{
	public class ProductPhotoCustomRepository : Repository<TblProductPhoto>, IProductPhotoCustomRepository
	{
		private readonly AppDbContext _context;

		public ProductPhotoCustomRepository(AppDbContext context) : base(context)
		{
			_context = context;
		}

		public IQueryable<TblProductPhoto> ReadPhotos()
		{
			var query = _context.TblProductPhotos.Include(e => e.TblProduct).ThenInclude(e => e.Merchant).ThenInclude(e => e.User);
			return query;
		}
	}
}
