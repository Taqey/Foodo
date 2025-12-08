using Foodo.Domain.Entities;
using Foodo.Domain.Enums;
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
		public virtual DbSet<TblRestaurantCategory> TblRestaurantCategories { get; set; }
		public virtual DbSet<TblCategoryOfRestaurant> TblCategoryOfRestaurants { get; set; }
		public virtual DbSet<TblCategoryOfProduct> TblCategoryOfProducts { get; set; }
		public virtual DbSet<TblProductCategory> TblProductCategories { get; set; }
		public virtual DbSet<TblDriver> TblDrivers { get; set; }
		public virtual DbSet<LkpUserPhoto> LkpUsersPhotos { get; set; }
		public virtual DbSet<TblProductPhoto> TblProductPhotos { get; set; }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			var Roles = new List<IdentityRole>
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
			modelBuilder.Entity<TblCategoryOfRestaurant>().HasData(
		Enum.GetValues(typeof(RestaurantCategory))
			.Cast<RestaurantCategory>()
			.Select(c => new TblCategoryOfRestaurant
			{
				CategoryId = (int)c,
				CategoryName = c.ToString()
			})
);
			modelBuilder.Entity<TblCategoryOfProduct>().HasData(
				Enum.GetValues(typeof(FoodCategory))
					.Cast<FoodCategory>()
					.Select(c => new TblCategoryOfProduct
					{
						CategoryId = (int)c,
						CategoryName = c.ToString()
					})
		);

			modelBuilder.Entity<LkpCode>(entity =>
			{
				entity.Property(e => e.Key).HasMaxLength(50);
				entity.Property(e => e.CodeType).HasConversion<string>().HasMaxLength(50);
			});
			modelBuilder.Entity<IdentityRole>().HasData(Roles);
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<LkpAttribute>(entity =>
			{
				entity.ToTable("LkpAttributes");

				entity.HasKey(e => e.AttributeId);

				entity.Property(e => e.CreatedDate)
					.HasDefaultValueSql("(getdate())", "DF_LkpAttributes_CreatedDate")
					.HasColumnType("datetime");
				entity.Property(e => e.DeletedDate).HasColumnType("datetime");
				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(50);
				entity.Property(e => e.value)
	.IsRequired()
	.HasMaxLength(50);
			});

			modelBuilder.Entity<LkpMeasureUnit>(entity =>
			{
				entity.ToTable("LkpMeasureUnits");

				entity.HasKey(e => e.UnitOfMeasureId);
				entity.Property(e => e.UnitOfMeasureName)
	.IsRequired()
	.HasMaxLength(50);
				entity.Property(e => e.CreatedDate)
					.HasDefaultValueSql("(getdate())", "DF_LkpMeasureUnits_CreatedDate")
					.HasColumnType("datetime");
				entity.Property(e => e.DeletedDate).HasColumnType("datetime");
			});

			modelBuilder.Entity<LkpProductDetailsAttribute>(entity =>
			{
				entity.ToTable("LkpProductDetailsAttributes");

				entity.HasKey(e => e.ProductDetailAttributeId);


				entity.HasOne(d => d.Attribute).WithMany(p => p.LkpProductDetailsAttributes)
					.HasForeignKey(d => d.AttributeId)
					.HasConstraintName("FK_LkpProductDetailsAttributes_LkpAttributes").OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(d => d.ProductDetail).WithMany(p => p.LkpProductDetailsAttributes)
					.HasForeignKey(d => d.ProductDetailId)
					.HasConstraintName("FK_LkpProductDetailsAttributes_TblProductDetails");

				entity.HasOne(d => d.UnitOfMeasure).WithMany(p => p.LkpProductDetailsAttributes)
					.HasForeignKey(d => d.UnitOfMeasureId)
					.HasConstraintName("FK_LkpProductDetailsAttributes_LkpMeasureUnits").OnDelete(DeleteBehavior.Cascade);
			});

			modelBuilder.Entity<TblAdress>(entity =>
			{
				entity.ToTable("TblAdresses");

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
				entity.ToTable("TblCustomers");

				entity.HasKey(e => e.UserId);


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
				entity.ToTable("TblMerchants");

				entity.HasKey(e => e.UserId);


				entity.Property(e => e.StoreDescription)
					.IsRequired()
					.HasMaxLength(250);
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
				entity.ToTable("TblOrders");

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
					.HasMaxLength(50).HasConversion<string>();
				entity.Property(e => e.Tax).HasColumnType("decimal(9, 2)");
				entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");
				entity.Property(e => e.CustomerId).HasColumnName("CustomerId")
					.IsRequired()
					.HasMaxLength(450);
				entity.Property(e => e.MerchantId).HasColumnName("MerchantId")
	.IsRequired()
	.HasMaxLength(450);
				entity.Property(e => e.PaidMoney).HasColumnType("decimal(18, 2)");

			});

			modelBuilder.Entity<TblProduct>(entity =>
			{
				entity.ToTable("TblProducts");

				entity.HasKey(e => e.ProductId);

				entity.Property(e => e.CreatedDate)
					.HasDefaultValueSql("(getdate())", "DF_TblProducts_CreatedDate")
					.HasColumnType("datetime");
				entity.Property(e => e.DeletedDate).HasColumnType("datetime");
				entity.Property(e => e.ProductDescription)
					.IsRequired()
					.HasMaxLength(250);
				entity.Property(e => e.ProductsName)
					.IsRequired()
					.HasMaxLength(50);
				entity.Property(e => e.UserId)
					.IsRequired()
					.HasMaxLength(450);

				entity.HasOne(d => d.Merchant).WithMany(p => p.TblProducts)
					.HasForeignKey(d => d.UserId)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("FK_TblProducts_TblMerchant");
			});

			modelBuilder.Entity<TblProductDetail>(entity =>
			{
				entity.ToTable("TblProductDetails");

				entity.HasKey(e => e.ProductDetailId);

				entity.Property(e => e.Price).HasColumnType("decimal(9, 2)");

				entity.HasOne(d => d.Product).WithMany(p => p.TblProductDetails)
					.HasForeignKey(d => d.ProductId)
					.HasConstraintName("FK_TblProductDetails_TblProducts");
			});

			modelBuilder.Entity<TblProductsOrder>(entity =>
			{
				entity.ToTable("TblProductsOrders");

				entity.HasKey(e => e.ProductorderId);

				entity.Property(e => e.ProductorderId)
					.ValueGeneratedOnAdd()
					.HasColumnName("ProductOrderId");
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
			modelBuilder.Entity<TblProductCategory>(entity =>
			{
				entity.ToTable("TblProductCategories");

				entity.HasKey(pc => pc.productcategoryid);

				entity.HasOne(pc => pc.Product)
					.WithMany(p => p.ProductCategories)
					.HasForeignKey(pc => pc.productid);

				entity.HasOne(pc => pc.Category)
					.WithMany(c => c.ProductCategories)
					.HasForeignKey(pc => pc.categoryid);
			});

			modelBuilder.Entity<TblRestaurantCategory>(entity =>
			{
				entity.ToTable("TblRestaurantCategories");

				entity.HasKey(rc => rc.restaurantcategoryid);
				entity.HasOne(rc => rc.Restaurant)
					.WithMany(m => m.TblRestaurantCategories)
					.HasForeignKey(rc => rc.restaurantid);
				entity.HasOne(rc => rc.Category)
					.WithMany(c => c.RestaurantCategories)
					.HasForeignKey(rc => rc.categoryid);
			});
			modelBuilder.Entity<TblCategoryOfRestaurant>(entity =>
			{
				entity.ToTable("TblCategoryOfRestaurants");

				entity.HasKey(e => e.CategoryId);
				entity.Property(e => e.CategoryName)
					.IsRequired()
					.HasMaxLength(50);
			});
			modelBuilder.Entity<TblCategoryOfProduct>(entity =>
			{
				entity.ToTable("TblCategoryOfProducts");

				entity.HasKey(e => e.CategoryId);
				entity.Property(e => e.CategoryName)
					.IsRequired()
					.HasMaxLength(50);
			});
			modelBuilder.Entity<TblDriver>(entity =>
			{
				entity.ToTable("TblDrivers");

				entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
				entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
				entity.HasKey(e => e.UserId);
				entity.HasOne(e => e.User).WithOne(e => e.TblDriver).HasForeignKey<TblDriver>(e => e.UserId);
			});
			modelBuilder.Entity<LkpUserPhoto>(entity =>
			{
				entity.ToTable("LkpUserPhotos");
				entity.HasKey(e => e.UserId);
				entity.Property(e => e.UserId).HasMaxLength(450);
				entity.Property(e => e.Url).HasMaxLength(450).IsRequired();
				entity.HasOne(e => e.user).WithOne(e => e.UserPhoto).HasForeignKey<LkpUserPhoto>(e => e.UserId);


			});
			modelBuilder.Entity<TblProductPhoto>(entity =>
			{
				entity.ToTable("TblProductPhotos");
				modelBuilder.Entity<TblProductPhoto>()
	.HasOne(p => p.TblProduct)
	.WithMany(p => p.ProductPhotos)
	.HasForeignKey(p => p.ProductId)
	.OnDelete(DeleteBehavior.Cascade);

				entity.HasKey(e => e.Id);
				entity.Property(e => e.Url).HasMaxLength(450).IsRequired();


			});
			OnModelCreatingPartial(modelBuilder);
		}

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}
