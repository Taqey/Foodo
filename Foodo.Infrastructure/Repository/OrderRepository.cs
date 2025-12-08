using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using Foodo.Infrastructure.Perisistence;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Infrastructure.Repository
{
	public class OrderRepository : Repository<TblOrder>, IOrderRepository
	{
		private readonly AppDbContext _context;

		public OrderRepository(AppDbContext context) : base(context)
		{
			_context = context;
		}

		public IQueryable<TblOrder> ReadOrders()
		{
			var query = _context.TblOrders;
			return query;
		}
		public IQueryable<TblOrder> ReadOrdersInclude()
		{
			var query = _context.TblOrders
				.Include(e => e.TblProductsOrders)
					.ThenInclude(po => po.Product)
						.ThenInclude(p => p.Merchant)
							.ThenInclude(u => u.User).
								ThenInclude(a => a.TblAdresses);
			return query;
		}
	}
}
