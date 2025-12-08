using Foodo.Domain.Entities;

namespace Foodo.Domain.Repository
{
	public interface IProductPhotoCustomRepository : IRepository<TblProductPhoto>
	{
		IQueryable<TblProductPhoto> ReadPhotos();
	}
}
