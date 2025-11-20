using Foodo.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Infrastructure.Perisistence
{
	public partial class AppDbContext : IdentityDbContext<ApplicationUser>
	{
		public AppDbContext()
		{
		}

		public AppDbContext(DbContextOptions<AppDbContext> options)
			: base(options)
		{
		}

		public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }

		public virtual DbSet<LkpAttribute> LkpAttributes { get; set; }

		public virtual DbSet<LkpMeasureUnit> LkpMeasureUnits { get; set; }

		public virtual DbSet<LkpProductDetailsAttribute> LkpProductDetailsAttributes { get; set; }

		public virtual DbSet<TblAdress> TblAdresses { get; set; }

		public virtual DbSet<TblCustomer> TblCustomers { get; set; }

		public virtual DbSet<TblMerchant> TblMerchants { get; set; }

		public virtual DbSet<TblOrder> TblOrders { get; set; }

		public virtual DbSet<TblProduct> TblProducts { get; set; }

		public virtual DbSet<TblProductDetail> TblProductDetails { get; set; }

		public virtual DbSet<TblProductsOrder> TblProductsOrders { get; set; }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			var Roles=new List<IdentityRole>
			{
				new IdentityRole
				{
					Id="d6d2f11c-6482-4b9f-8e59-24e997ac635b",
					Name="Customer",
					NormalizedName="CUSTOMER",
					ConcurrencyStamp="d6d2f11c-6482-4b9f-8e59-24e997ac635b"
				},
				new IdentityRole
				{
					Id="23fd934c-7bcf-40e0-a41e-a253a2d3b557",
					Name="Merchant",
					NormalizedName="MERCHANT",
					ConcurrencyStamp="23fd934c-7bcf-40e0-a41e-a253a2d3b557"
				}
			};
			modelBuilder.Entity<LkpCodes>(entity => {
				entity.Property(e => e.Key).HasMaxLength(50);
				entity.Property(e => e.CodeType).HasConversion<string>().HasMaxLength(50);
			});
			modelBuilder.Entity<IdentityRole>().HasData(Roles);
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<LkpAttribute>(entity =>
			{
				entity.HasKey(e => e.AttributeId);

				entity.Property(e => e.AttributeId).ValueGeneratedNever();
				entity.Property(e => e.CreatedDate)
					.HasDefaultValueSql("(getdate())", "DF_LkpAttributes_CreatedDate")
					.HasColumnType("datetime");
				entity.Property(e => e.DeletedDate).HasColumnType("datetime");
				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(50);
			});

			modelBuilder.Entity<LkpMeasureUnit>(entity =>
			{
				entity.HasKey(e => e.UnitOfMeasureId);

				entity.Property(e => e.UnitOfMeasureId).ValueGeneratedNever();
				entity.Property(e => e.CreatedDate)
					.HasDefaultValueSql("(getdate())", "DF_LkpMeasureUnits_CreatedDate")
					.HasColumnType("datetime");
				entity.Property(e => e.DeletedDate).HasColumnType("datetime");
			});

			modelBuilder.Entity<LkpProductDetailsAttribute>(entity =>
			{
				entity.HasKey(e => e.ProductDetailAttributeId);

				entity.Property(e => e.ProductDetailAttributeId).ValueGeneratedNever();

				entity.HasOne(d => d.Attribute).WithMany(p => p.LkpProductDetailsAttributes)
					.HasForeignKey(d => d.AttributeId)
					.HasConstraintName("FK_LkpProductDetailsAttributes_LkpAttributes");

				entity.HasOne(d => d.ProductDetail).WithMany(p => p.LkpProductDetailsAttributes)
					.HasForeignKey(d => d.ProductDetailId)
					.HasConstraintName("FK_LkpProductDetailsAttributes_TblProductDetails");

				entity.HasOne(d => d.UnitOfMeasure).WithMany(p => p.LkpProductDetailsAttributes)
					.HasForeignKey(d => d.UnitOfMeasureId)
					.HasConstraintName("FK_LkpProductDetailsAttributes_LkpMeasureUnits");
			});

			modelBuilder.Entity<TblAdress>(entity =>
			{
				entity.HasKey(e => e.AddressId);

				entity.Property(e => e.City).HasMaxLength(50);
				entity.Property(e => e.Country).HasMaxLength(50);
				entity.Property(e => e.CreatedDate)
					.HasDefaultValueSql("(getdate())", "DF_TblAdresses_CreatedDate")
					.HasColumnType("datetime");
				entity.Property(e => e.DeletedDate).HasColumnType("datetime");
				entity.Property(e => e.IsDefault).HasDefaultValue(false, "DF_TblAdresses_IsDefault");
				entity.Property(e => e.PostalCode).HasMaxLength(50);
				entity.Property(e => e.State).HasMaxLength(50);
				entity.Property(e => e.StreetAddress).HasMaxLength(50);
				entity.Property(e => e.UserId).HasMaxLength(450);

				entity.HasOne(d => d.User).WithMany(p => p.TblAdresses)
					.HasForeignKey(d => d.UserId)
					.HasConstraintName("FK_TblAdresses_ApplicationUser");
			});

			modelBuilder.Entity<TblCustomer>(entity =>
			{
				entity.HasKey(e => e.UserId);

				entity.ToTable("TblCustomer");

				entity.Property(e => e.FirstName)
					.IsRequired()
					.HasMaxLength(50);
				entity.Property(e => e.Gender)
					.IsRequired()
					.HasMaxLength(50);
				entity.Property(e => e.LastName)
					.IsRequired()
					.HasMaxLength(50);

				entity.HasOne(d => d.User).WithOne(p => p.TblCustomer)
					.HasForeignKey<TblCustomer>(d => d.UserId)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("FK_TblCustomer_ApplicationUser");
			});

			modelBuilder.Entity<TblMerchant>(entity =>
			{
				entity.HasKey(e => e.UserId);

				entity.ToTable("TblMerchant");

				entity.Property(e => e.StoreDescription)
					.IsRequired()
					.HasMaxLength(100);
				entity.Property(e => e.StoreName)
					.IsRequired()
					.HasMaxLength(50);

				entity.HasOne(d => d.User).WithOne(p => p.TblMerchant)
					.HasForeignKey<TblMerchant>(d => d.UserId)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("FK_TblMerchant_ApplicationUser");
			});

			modelBuilder.Entity<TblOrder>(entity =>
			{
				entity.HasKey(e => e.OrderId);

				entity.Property(e => e.CreatedDate)
					.HasDefaultValueSql("(getdate())", "DF_TblOrders_CreatedDate")
					.HasColumnType("datetime");
				entity.Property(e => e.DeletedDate).HasColumnType("datetime");
				entity.Property(e => e.OrderDate)
					.HasDefaultValueSql("(getdate())", "DF_TblOrders_OrderDate")
					.HasColumnType("datetime");
				entity.Property(e => e.OrderStatus)
					.IsRequired()
					.HasMaxLength(50);
				entity.Property(e => e.Tax).HasColumnType("decimal(9, 2)");
				entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");
				entity.Property(e => e.UserId)
					.IsRequired()
					.HasMaxLength(450);
			});

			modelBuilder.Entity<TblProduct>(entity =>
			{
				entity.HasKey(e => e.ProductId);

				entity.Property(e => e.CreatedDate)
					.HasDefaultValueSql("(getdate())", "DF_TblProducts_CreatedDate")
					.HasColumnType("datetime");
				entity.Property(e => e.DeletedDate).HasColumnType("datetime");
				entity.Property(e => e.ProductDescription)
					.IsRequired()
					.HasMaxLength(100);
				entity.Property(e => e.ProductsName)
					.IsRequired()
					.HasMaxLength(50);
				entity.Property(e => e.UserId)
					.IsRequired()
					.HasMaxLength(450);

				entity.HasOne(d => d.User).WithMany(p => p.TblProducts)
					.HasForeignKey(d => d.UserId)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("FK_TblProducts_TblMerchant");
			});

			modelBuilder.Entity<TblProductDetail>(entity =>
			{
				entity.HasKey(e => e.ProductDetailId);

				entity.Property(e => e.Price).HasColumnType("decimal(9, 2)");

				entity.HasOne(d => d.Product).WithMany(p => p.TblProductDetails)
					.HasForeignKey(d => d.ProductId)
					.HasConstraintName("FK_TblProductDetails_TblProducts");
			});

			modelBuilder.Entity<TblProductsOrder>(entity =>
			{
				entity.HasKey(e => e.ProductIorderId);

				entity.Property(e => e.ProductIorderId)
					.ValueGeneratedNever()
					.HasColumnName("ProductIOrderId");
				entity.Property(e => e.CreatedDate)
					.HasDefaultValueSql("(getdate())", "DF_TblProductsOrders_CreatedDate")
					.HasColumnType("datetime");
				entity.Property(e => e.DeletedDate).HasColumnType("datetime");
				entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

				entity.HasOne(d => d.Order).WithMany(p => p.TblProductsOrders)
					.HasForeignKey(d => d.OrderId)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("FK_TblProductsOrders_TblOrders");

				entity.HasOne(d => d.Product).WithMany(p => p.TblProductsOrders)
					.HasForeignKey(d => d.ProductId)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("FK_TblProductsOrders_TblProducts");
			});

			OnModelCreatingPartial(modelBuilder);
		}

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}
