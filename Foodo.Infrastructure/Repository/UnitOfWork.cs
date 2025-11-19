using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using Foodo.Infrastructure.Perisistence;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Foodo.Infrastructure.Repository
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly AppDbContext _context;

		public IRepository<LkpAttribute> AttributeRepository { get; }
		public IRepository<LkpMeasureUnit> MeasureUnitRepository { get; }
		public IRepository<LkpProductDetailsAttribute> ProductDetailsAttributeRepository { get; }
		public IRepository<TblAdress> AdressRepository { get; }
		public IRepository<TblCustomer> CustomerRepository { get; }
		public IRepository<TblMerchant> MerchantRepository { get; }
		public IRepository<TblOrder> OrderRepository { get; }
		public IRepository<TblProduct> ProductRepository { get; }
		public IRepository<TblProductDetail> ProductDetailRepository { get; }
		public IRepository<TblProductsOrder> ProductsOrderRepository { get; }

		public UnitOfWork(
			AppDbContext context,
			IRepository<LkpAttribute> AttributeRepository,
			IRepository<LkpMeasureUnit> MeasureUnitRepository,
			IRepository<LkpProductDetailsAttribute> ProductDetailsAttributeRepository,
			IRepository<TblAdress> AdressRepository,
			IRepository<TblCustomer> CustomerRepository,
			IRepository<TblMerchant> MerchantRepository,
			IRepository<TblOrder> OrderRepository,
			IRepository<TblProduct> ProductRepository,
			IRepository<TblProductDetail> ProductDetailRepository,
			IRepository<TblProductsOrder> ProductsOrderRepository
		)
		{
			_context = context;

			this.AttributeRepository = AttributeRepository;
			this.MeasureUnitRepository = MeasureUnitRepository;
			this.ProductDetailsAttributeRepository = ProductDetailsAttributeRepository;
			this.AdressRepository = AdressRepository;
			this.CustomerRepository = CustomerRepository;
			this.MerchantRepository = MerchantRepository;
			this.OrderRepository = OrderRepository;
			this.ProductRepository = ProductRepository;
			this.ProductDetailRepository = ProductDetailRepository;
			this.ProductsOrderRepository = ProductsOrderRepository;
		}

		public async Task<int> saveAsync()
		{
			return await _context.SaveChangesAsync();
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}
