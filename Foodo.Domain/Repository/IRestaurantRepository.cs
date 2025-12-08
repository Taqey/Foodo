using Foodo.Domain.Entities;

namespace Foodo.Domain.Repository
{
	public interface IRestaurantRepository : IRepository<TblMerchant>
	{
		IQueryable<TblMerchant> ReadRestaurants();
		IQueryable<TblMerchant> ReadRestaurantsInclude();

	}
}
