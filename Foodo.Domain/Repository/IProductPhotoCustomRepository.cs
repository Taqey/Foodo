using Foodo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Domain.Repository
{
	public interface IProductPhotoCustomRepository:IRepository<TblProductPhoto>
	{
		IQueryable<TblProductPhoto> ReadPhotos();
	}
}
