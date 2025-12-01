using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using Foodo.Infrastructure.Perisistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Infrastructure.Repository
{
	public class UserRepository : Repository<ApplicationUser>, IUserRepository
	{
		private readonly AppDbContext _context;

		public UserRepository(AppDbContext context) : base(context)
		{
			_context = context;
		}

		public IQueryable<ApplicationUser> ReadCustomer()
		{
			var query = _context.ApplicationUsers.Include(e => e.TblCustomer).Include(e => e.TblAdresses).Include(e => e.UserPhoto);
			return query;
		}

		public IQueryable<ApplicationUser> ReadMerchants()
		{
			var query = _context.ApplicationUsers.Include(e => e.TblMerchant).ThenInclude(e => e.TblRestaurantCategories).ThenInclude(e => e.Category).Include(e => e.TblAdresses).Include(e => e.UserPhoto);

			return query;
		}
	}
}
