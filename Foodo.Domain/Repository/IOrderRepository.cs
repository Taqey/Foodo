using Foodo.Domain.Entities;

namespace Foodo.Domain.Repository
{
	public interface IOrderRepository : IRepository<TblOrder>
	{
		IQueryable<TblOrder> ReadOrders();
		IQueryable<TblOrder> ReadOrdersInclude();

	}
}
