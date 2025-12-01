using Foodo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Domain.Repository
{
	public interface IUserRepository:IRepository<ApplicationUser>
	{
		IQueryable<ApplicationUser> ReadMerchants();
		IQueryable<ApplicationUser> ReadCustomer();

	}
}
