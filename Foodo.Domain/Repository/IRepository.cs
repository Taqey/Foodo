using System.Linq.Expressions;

namespace Foodo.Domain.Repository
{
	public interface IRepository<T> where T : class
	{
		Task<T> CreateAsync(T entity);
		Task<IEnumerable<T>> CreateRangeAsync(IEnumerable<T> entities);
		Task<T> ReadByIdAsync(int id);
		Task<IEnumerable<T>> ReadAllAsync();
		Task<IEnumerable<T>> ReadAllIncludingAsync(params Expression<Func<T, object>>[] includes);
		void Update(T entity);
		void Attach(T entity);
		void Delete(T entity);
		void DeleteAll();
		Task <T> FindByContidtionAsync(Expression<Func<T, bool>> expression);
		Task<IEnumerable<T>> FindAllByContidtionAsync(Expression<Func<T, bool>> expression);
		//Task<IEnumerable<T>> FindAllByContidtionIncludingAsync(Expression<Func<T, bool>> expression,Func<IQueryable<T>, IQueryable<T>> include);
		Task<(IEnumerable<T> Items, int TotalCount, int TotalPages)> PaginationAsync(int page = 1, int pageSize = 10, params Expression<Func<T, bool>>[] filters);
		void DeleteRange(IEnumerable<T> entities);

	}
}
