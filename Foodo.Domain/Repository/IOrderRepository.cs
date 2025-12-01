using Foodo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Domain.Repository
{
	public interface IOrderRepository : IRepository<TblOrder>
	{
		IQueryable<TblOrder> ReadOrders();
		IQueryable<TblOrder> ReadOrdersInclude();

	}
}
