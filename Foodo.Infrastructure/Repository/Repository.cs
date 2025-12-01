using Foodo.Domain.Repository;
using Foodo.Infrastructure.Perisistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Foodo.Infrastructure.Repository
{
	public class Repository<T> : IRepository<T> where T : class
	{
		private readonly AppDbContext _context;
		DbSet<T> _dbSet;
		public Repository(AppDbContext context)
		{
			_context = context;
			_dbSet = _context.Set<T>();
		}
		public async Task<T> CreateAsync(T entity)
		{
			var result = await _dbSet.AddAsync(entity);
			return result.Entity;
		}
		public async Task<T> ReadByIdAsync(int id)
		{
			var entity = await _dbSet.FindAsync(id);
			return entity;
		}
		public async Task<IEnumerable<T>> ReadAllAsync()
		{
			var entities = await _dbSet.AsNoTracking().ToListAsync();
			return entities;
		}

		public async Task<IEnumerable<T>> ReadAllIncludingAsync(params Expression<Func<T, object>>[] includes)
		{
			IQueryable<T> query = _dbSet.AsQueryable();
			foreach (var include in includes)
			{
				query = query.Include(include);
			}
			return await query.AsNoTracking().AsNoTracking().ToListAsync();
		}
		public void Update(T entity)
		{

			_context.Entry(entity).State = EntityState.Modified;
		}
		public void Attach(T entity)
		{
			_dbSet.Attach(entity);
		}

		public void Delete(T entity)
		{
			_context.Entry(entity).State = EntityState.Deleted;
		}

		public void DeleteAll()
		{
			_dbSet.RemoveRange(_dbSet);
		}

		public async Task<IEnumerable<T>> FindAllByContidtionAsync(Expression<Func<T, bool>> expression)
		{
			var List = await _dbSet.Where(expression).AsNoTracking().ToListAsync();
			return List;
		}

		//public async Task<IEnumerable<T>> FindAllByContidtionIncludingAsync(Expression<Func<T, bool>> expression,Func<IQueryable<T>, IQueryable<T>> include)
		//{
		//	IQueryable<T> query = _dbSet.AsQueryable();

		//	query = include(query);
		//	return await query.Where(expression).ToListAsync();
		//	}
		public void DeleteRange(IEnumerable<T> entities)
		{
			_dbSet.RemoveRange(entities);
		}

		public async Task<T> FindByContidtionAsync(Expression<Func<T, bool>> expression)
		{
			var entity = await _dbSet.FirstOrDefaultAsync(expression);
			return entity;
		}

		public async Task<(IEnumerable<T> Items, int TotalCount, int TotalPages)>PaginationAsync(int page = 1, int pageSize = 10, params Expression<Func<T, bool>>[] filters)
		{
			IQueryable<T> query = _dbSet.AsQueryable();

			if (filters != null)
			{
				foreach (var filter in filters)
					query = query.Where(filter);
			}
			var totalCount = await query.CountAsync();
			var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

			var items = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.AsNoTracking().ToListAsync();

			return (items, totalCount, totalPages);
		}


		public async Task<IEnumerable<T>> CreateRangeAsync(IEnumerable<T> entities)
		{
			await _dbSet.AddRangeAsync(entities);
			return entities;
		}
	}
}
