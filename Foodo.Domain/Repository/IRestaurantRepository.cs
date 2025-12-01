using Foodo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Domain.Repository
{
	public interface IRestaurantRepository :IRepository<TblMerchant>
	{
		IQueryable<TblMerchant> ReadRestaurants();
		IQueryable<TblMerchant> ReadRestaurantsInclude();

	}
}
