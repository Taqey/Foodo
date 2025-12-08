using Foodo.Domain.Entities;

namespace Foodo.Domain.Repository
{
	public interface IUserRepository : IRepository<ApplicationUser>
	{
		IQueryable<ApplicationUser> ReadMerchants();
		IQueryable<ApplicationUser> ReadCustomer();

	}
}
