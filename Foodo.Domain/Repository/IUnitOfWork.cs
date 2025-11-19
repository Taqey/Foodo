using Foodo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Domain.Repository
{
	public interface IUnitOfWork:IDisposable
	{
		public IRepository<LkpAttribute> AttributeRepository { get; }
		public IRepository<LkpMeasureUnit> MeasureUnitRepository { get; }
		public IRepository<LkpProductDetailsAttribute> ProductDetailsAttributeRepository { get; }
		public IRepository<TblAdress> AdressRepository { get; }
		public IRepository<TblCustomer> CustomerRepository { get; }
		public IRepository<TblMerchant> MerchantRepository { get; }
		public IRepository<TblOrder > OrderRepository { get; }
		public IRepository<TblProduct > ProductRepository { get; }
		public IRepository<TblProductDetail> ProductDetailRepository { get; }
		public IRepository<TblProductsOrder> ProductsOrderRepository { get; }
		Task<int> saveAsync();





	}
}
