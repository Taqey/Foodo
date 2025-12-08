using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using Foodo.Infrastructure.Perisistence;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Infrastructure.Repository
{
	public class RestaurantRepository : Repository<TblMerchant>, IRestaurantRepository
	{
		private readonly AppDbContext _context;

		public RestaurantRepository(AppDbContext context) : base(context)
		{
			_context = context;
		}

		public IQueryable<TblMerchant> ReadRestaurants()
		{
			var query = _context.TblMerchants;

			return query;
		}
		public IQueryable<TblMerchant> ReadRestaurantsInclude()
		{
			var query = _context.TblMerchants
				.Include(e => e.TblRestaurantCategories)
				.ThenInclude(c => c.Category)
				.Include(e => e.User)
				.ThenInclude(e => e.UserPhoto)
				.AsNoTracking();
			return query;
		}
	}
}
