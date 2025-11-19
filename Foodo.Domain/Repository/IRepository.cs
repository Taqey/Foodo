using System.Linq.Expressions;

namespace Foodo.Domain.Repository
{
	public interface IRepository<T> where T : class
	{
		Task CreateAsync(T entity);
		Task<T> ReadByIdAsync(int id);
		Task<IEnumerable<T>> ReadAllAsync();
		Task<IEnumerable<T>> ReadAllIncludingAsync(params Expression<Func<T, object>>[] includes);
		void Update(T entity);
		void Delete(T entity);
		void DeleteAll();
		Task <T> FindByContidtionAsync(Expression<Func<T, bool>> expression);
		Task<IEnumerable<T>> FindAllByContidtionAsync(Expression<Func<T, bool>> expression);
		Task<IEnumerable<T>> PaginationAsync(int page, int pagesize);


	}
}
