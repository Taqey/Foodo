using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Foodo.Infrastructure.Migrations
{
	/// <inheritdoc />
	public partial class clean : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "AspNetRoles",
				columns: table => new
				{
					Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
					Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
					NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
					ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetRoles", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUsers",
				columns: table => new
				{
					Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
					UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
					NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
					Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
					NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
					EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
					PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
					SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
					ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
					PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
					PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
					TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
					LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
					LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
					AccessFailedCount = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUsers", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "LkpAttributes",
				columns: table => new
				{
					AttributeId = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					DeletedDate = table.Column<DateTime>(type: "datetime", nullable: true),
					Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
					value = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
					CreatedBy = table.Column<int>(type: "int", nullable: true),
					IsDeleted = table.Column<bool>(type: "bit", nullable: false),
					CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
						.Annotation("Relational:DefaultConstraintName", "DF_LkpAttributes_CreatedDate"),
					UpdatedBy = table.Column<int>(type: "int", nullable: true),
					DeletedBy = table.Column<int>(type: "int", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_LkpAttributes", x => x.AttributeId);
				});

			migrationBuilder.CreateTable(
				name: "LkpMeasureUnits",
				columns: table => new
				{
					UnitOfMeasureId = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					UnitOfMeasureName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
					CreatedBy = table.Column<int>(type: "int", nullable: true),
					IsDeleted = table.Column<bool>(type: "bit", nullable: false),
					CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
						.Annotation("Relational:DefaultConstraintName", "DF_LkpMeasureUnits_CreatedDate"),
					UpdatedBy = table.Column<int>(type: "int", nullable: true),
					DeletedBy = table.Column<int>(type: "int", nullable: true),
					DeletedDate = table.Column<DateTime>(type: "datetime", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_LkpMeasureUnits", x => x.UnitOfMeasureId);
				});

			migrationBuilder.CreateTable(
				name: "TblCategoryOfProducts",
				columns: table => new
				{
					CategoryId = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					CategoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TblCategoryOfProducts", x => x.CategoryId);
				});

			migrationBuilder.CreateTable(
				name: "TblCategoryOfRestaurants",
				columns: table => new
				{
					CategoryId = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					CategoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TblCategoryOfRestaurants", x => x.CategoryId);
				});

			migrationBuilder.CreateTable(
				name: "TblOrders",
				columns: table => new
				{
					OrderId = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					DeletedDate = table.Column<DateTime>(type: "datetime", nullable: true),
					OrderDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
						.Annotation("Relational:DefaultConstraintName", "DF_TblOrders_OrderDate"),
					TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
					Tax = table.Column<decimal>(type: "decimal(9,2)", nullable: true),
					CreatedBy = table.Column<int>(type: "int", nullable: true),
					IsDeleted = table.Column<bool>(type: "bit", nullable: false),
					CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
						.Annotation("Relational:DefaultConstraintName", "DF_TblOrders_CreatedDate"),
					UpdatedBy = table.Column<int>(type: "int", nullable: true),
					DeletedBy = table.Column<int>(type: "int", nullable: true),
					OrderStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
					BillingAddressId = table.Column<int>(type: "int", nullable: false),
					DriverId = table.Column<string>(type: "nvarchar(max)", nullable: true),
					PaidMoney = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
					CustomerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
					MerchantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TblOrders", x => x.OrderId);
				});

			migrationBuilder.CreateTable(
				name: "AspNetRoleClaims",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
					ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
					ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
					table.ForeignKey(
						name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
						column: x => x.RoleId,
						principalTable: "AspNetRoles",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUserClaims",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
					ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
					ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
					table.ForeignKey(
						name: "FK_AspNetUserClaims_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUserLogins",
				columns: table => new
				{
					LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
					ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
					ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
					table.ForeignKey(
						name: "FK_AspNetUserLogins_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUserRoles",
				columns: table => new
				{
					UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
					RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
					table.ForeignKey(
						name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
						column: x => x.RoleId,
						principalTable: "AspNetRoles",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_AspNetUserRoles_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUserTokens",
				columns: table => new
				{
					UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
					LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
					Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
					Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
					table.ForeignKey(
						name: "FK_AspNetUserTokens_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "LkpCode",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
					CodeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					IsUsed = table.Column<bool>(type: "bit", nullable: true),
					ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_LkpCode", x => x.Id);
					table.ForeignKey(
						name: "FK_LkpCode_AspNetUsers_ApplicationUserId",
						column: x => x.ApplicationUserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id");
				});

			migrationBuilder.CreateTable(
				name: "lkpRefreshToken",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Token = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					RevokedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
					ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_lkpRefreshToken", x => x.Id);
					table.ForeignKey(
						name: "FK_lkpRefreshToken_AspNetUsers_ApplicationUserId",
						column: x => x.ApplicationUserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id");
				});

			migrationBuilder.CreateTable(
				name: "LkpUserPhotos",
				columns: table => new
				{
					UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
					Url = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_LkpUserPhotos", x => x.UserId);
					table.ForeignKey(
						name: "FK_LkpUserPhotos_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "TblAdresses",
				columns: table => new
				{
					AddressId = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
					City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
					State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
					StreetAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
					PostalCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
					Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
					IsDefault = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
						.Annotation("Relational:DefaultConstraintName", "DF_TblAdresses_IsDefault"),
					CreatedBy = table.Column<int>(type: "int", nullable: true),
					IsDeleted = table.Column<bool>(type: "bit", nullable: false),
					CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
						.Annotation("Relational:DefaultConstraintName", "DF_TblAdresses_CreatedDate"),
					UpdatedBy = table.Column<int>(type: "int", nullable: true),
					DeletedBy = table.Column<int>(type: "int", nullable: true),
					DeletedDate = table.Column<DateTime>(type: "datetime", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TblAdresses", x => x.AddressId);
					table.ForeignKey(
						name: "FK_TblAdresses_ApplicationUser",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id");
				});

			migrationBuilder.CreateTable(
				name: "TblCustomers",
				columns: table => new
				{
					UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
					FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
					LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
					Gender = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
					BirthDate = table.Column<DateOnly>(type: "date", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TblCustomers", x => x.UserId);
					table.ForeignKey(
						name: "FK_TblCustomer_ApplicationUser",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id");
				});

			migrationBuilder.CreateTable(
				name: "TblDrivers",
				columns: table => new
				{
					UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
					FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
					LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
					Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
					BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
					NationalId = table.Column<string>(type: "nvarchar(max)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TblDrivers", x => x.UserId);
					table.ForeignKey(
						name: "FK_TblDrivers_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "TblMerchants",
				columns: table => new
				{
					UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
					StoreName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
					StoreDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TblMerchants", x => x.UserId);
					table.ForeignKey(
						name: "FK_TblMerchant_ApplicationUser",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id");
				});

			migrationBuilder.CreateTable(
				name: "TblProducts",
				columns: table => new
				{
					ProductId = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ProductsName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
					ProductDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
					CreatedBy = table.Column<int>(type: "int", nullable: true),
					IsDeleted = table.Column<bool>(type: "bit", nullable: false),
					CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
						.Annotation("Relational:DefaultConstraintName", "DF_TblProducts_CreatedDate"),
					UpdatedBy = table.Column<int>(type: "int", nullable: true),
					DeletedBy = table.Column<int>(type: "int", nullable: true),
					DeletedDate = table.Column<DateTime>(type: "datetime", nullable: true),
					UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TblProducts", x => x.ProductId);
					table.ForeignKey(
						name: "FK_TblProducts_TblMerchant",
						column: x => x.UserId,
						principalTable: "TblMerchants",
						principalColumn: "UserId");
				});

			migrationBuilder.CreateTable(
				name: "TblRestaurantCategories",
				columns: table => new
				{
					restaurantcategoryid = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					restaurantid = table.Column<string>(type: "nvarchar(450)", nullable: false),
					categoryid = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TblRestaurantCategories", x => x.restaurantcategoryid);
					table.ForeignKey(
						name: "FK_TblRestaurantCategories_TblCategoryOfRestaurants_categoryid",
						column: x => x.categoryid,
						principalTable: "TblCategoryOfRestaurants",
						principalColumn: "CategoryId",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_TblRestaurantCategories_TblMerchants_restaurantid",
						column: x => x.restaurantid,
						principalTable: "TblMerchants",
						principalColumn: "UserId",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "TblProductCategories",
				columns: table => new
				{
					productcategoryid = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					productid = table.Column<int>(type: "int", nullable: false),
					categoryid = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TblProductCategories", x => x.productcategoryid);
					table.ForeignKey(
						name: "FK_TblProductCategories_TblCategoryOfProducts_categoryid",
						column: x => x.categoryid,
						principalTable: "TblCategoryOfProducts",
						principalColumn: "CategoryId",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_TblProductCategories_TblProducts_productid",
						column: x => x.productid,
						principalTable: "TblProducts",
						principalColumn: "ProductId",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "TblProductDetails",
				columns: table => new
				{
					ProductDetailId = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ProductId = table.Column<int>(type: "int", nullable: false),
					Price = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
					CreatedBy = table.Column<int>(type: "int", nullable: true),
					IsDeleted = table.Column<bool>(type: "bit", nullable: false),
					CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedBy = table.Column<int>(type: "int", nullable: true),
					DeletedBy = table.Column<int>(type: "int", nullable: true),
					DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TblProductDetails", x => x.ProductDetailId);
					table.ForeignKey(
						name: "FK_TblProductDetails_TblProducts",
						column: x => x.ProductId,
						principalTable: "TblProducts",
						principalColumn: "ProductId",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "TblProductPhotos",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ProductId = table.Column<int>(type: "int", nullable: false),
					Url = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
					isMain = table.Column<bool>(type: "bit", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TblProductPhotos", x => x.Id);
					table.ForeignKey(
						name: "FK_TblProductPhotos_TblProducts_ProductId",
						column: x => x.ProductId,
						principalTable: "TblProducts",
						principalColumn: "ProductId",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "TblProductsOrders",
				columns: table => new
				{
					ProductOrderId = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ProductId = table.Column<int>(type: "int", nullable: false),
					OrderId = table.Column<int>(type: "int", nullable: false),
					DeletedDate = table.Column<DateTime>(type: "datetime", nullable: true),
					Quantity = table.Column<int>(type: "int", nullable: false),
					Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
					CreatedBy = table.Column<int>(type: "int", nullable: true),
					IsDeleted = table.Column<bool>(type: "bit", nullable: false),
					CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
						.Annotation("Relational:DefaultConstraintName", "DF_TblProductsOrders_CreatedDate"),
					UpdatedBy = table.Column<int>(type: "int", nullable: true),
					DeletedBy = table.Column<int>(type: "int", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TblProductsOrders", x => x.ProductOrderId);
					table.ForeignKey(
						name: "FK_TblProductsOrders_TblOrders",
						column: x => x.OrderId,
						principalTable: "TblOrders",
						principalColumn: "OrderId");
					table.ForeignKey(
						name: "FK_TblProductsOrders_TblProducts",
						column: x => x.ProductId,
						principalTable: "TblProducts",
						principalColumn: "ProductId");
				});

			migrationBuilder.CreateTable(
				name: "LkpProductDetailsAttributes",
				columns: table => new
				{
					ProductDetailAttributeId = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ProductDetailId = table.Column<int>(type: "int", nullable: false),
					UnitOfMeasureId = table.Column<int>(type: "int", nullable: false),
					AttributeId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_LkpProductDetailsAttributes", x => x.ProductDetailAttributeId);
					table.ForeignKey(
						name: "FK_LkpProductDetailsAttributes_LkpAttributes",
						column: x => x.AttributeId,
						principalTable: "LkpAttributes",
						principalColumn: "AttributeId",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_LkpProductDetailsAttributes_LkpMeasureUnits",
						column: x => x.UnitOfMeasureId,
						principalTable: "LkpMeasureUnits",
						principalColumn: "UnitOfMeasureId",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_LkpProductDetailsAttributes_TblProductDetails",
						column: x => x.ProductDetailId,
						principalTable: "TblProductDetails",
						principalColumn: "ProductDetailId",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.InsertData(
				table: "AspNetRoles",
				columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
				values: new object[,]
				{
					{ "23fd934c-7bcf-40e0-a41e-a253a2d3b557", "23fd934c-7bcf-40e0-a41e-a253a2d3b557", "Merchant", "MERCHANT" },
					{ "d6d2f11c-6482-4b9f-8e59-24e997ac635b", "d6d2f11c-6482-4b9f-8e59-24e997ac635b", "Customer", "CUSTOMER" }
				});

			migrationBuilder.InsertData(
				table: "TblCategoryOfProducts",
				columns: new[] { "CategoryId", "CategoryName" },
				values: new object[,]
				{
					{ 1, "Burger" },
					{ 2, "Pizza" },
					{ 3, "Pasta" },
					{ 4, "Sandwich" },
					{ 5, "Grill" },
					{ 6, "FriedChicken" },
					{ 7, "Seafood" },
					{ 8, "Salad" },
					{ 9, "Soup" },
					{ 10, "Dessert" },
					{ 11, "IceCream" },
					{ 12, "Juice" },
					{ 13, "Coffee" },
					{ 14, "Beverage" },
					{ 15, "Appetizer" },
					{ 16, "MainCourse" },
					{ 17, "SideDish" },
					{ 18, "Shawarma" },
					{ 19, "Kebab" },
					{ 20, "Sushi" },
					{ 21, "Tacos" },
					{ 22, "Noodles" },
					{ 23, "RiceDishes" },
					{ 24, "Pastry" },
					{ 25, "Breakfast" }
				});

			migrationBuilder.InsertData(
				table: "TblCategoryOfRestaurants",
				columns: new[] { "CategoryId", "CategoryName" },
				values: new object[,]
				{
					{ 1, "FastFood" },
					{ 2, "CasualDining" },
					{ 3, "FineDining" },
					{ 4, "Cafe" },
					{ 5, "Bakery" },
					{ 6, "DessertShop" },
					{ 7, "JuiceBar" },
					{ 8, "Seafood" },
					{ 9, "Steakhouse" },
					{ 10, "Pizzeria" },
					{ 11, "BBQ" },
					{ 12, "FamilyRestaurant" },
					{ 13, "HealthyFood" },
					{ 14, "Vegetarian" },
					{ 15, "Vegan" },
					{ 16, "FoodTruck" },
					{ 17, "Buffet" },
					{ 18, "Sandwiches" },
					{ 19, "Egyptian" },
					{ 20, "Italian" },
					{ 21, "American" },
					{ 22, "Mexican" },
					{ 23, "Turkish" },
					{ 24, "Chinese" },
					{ 25, "Japanese" },
					{ 26, "Indian" },
					{ 27, "Lebanese" },
					{ 28, "Syrian" },
					{ 29, "Greek" },
					{ 30, "French" }
				});

			migrationBuilder.CreateIndex(
				name: "IX_AspNetRoleClaims_RoleId",
				table: "AspNetRoleClaims",
				column: "RoleId");

			migrationBuilder.CreateIndex(
				name: "RoleNameIndex",
				table: "AspNetRoles",
				column: "NormalizedName",
				unique: true,
				filter: "[NormalizedName] IS NOT NULL");

			migrationBuilder.CreateIndex(
				name: "IX_AspNetUserClaims_UserId",
				table: "AspNetUserClaims",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_AspNetUserLogins_UserId",
				table: "AspNetUserLogins",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_AspNetUserRoles_RoleId",
				table: "AspNetUserRoles",
				column: "RoleId");

			migrationBuilder.CreateIndex(
				name: "EmailIndex",
				table: "AspNetUsers",
				column: "NormalizedEmail");

			migrationBuilder.CreateIndex(
				name: "UserNameIndex",
				table: "AspNetUsers",
				column: "NormalizedUserName",
				unique: true,
				filter: "[NormalizedUserName] IS NOT NULL");

			migrationBuilder.CreateIndex(
				name: "IX_LkpCode_ApplicationUserId",
				table: "LkpCode",
				column: "ApplicationUserId");

			migrationBuilder.CreateIndex(
				name: "IX_LkpProductDetailsAttributes_AttributeId",
				table: "LkpProductDetailsAttributes",
				column: "AttributeId");

			migrationBuilder.CreateIndex(
				name: "IX_LkpProductDetailsAttributes_ProductDetailId",
				table: "LkpProductDetailsAttributes",
				column: "ProductDetailId");

			migrationBuilder.CreateIndex(
				name: "IX_LkpProductDetailsAttributes_UnitOfMeasureId",
				table: "LkpProductDetailsAttributes",
				column: "UnitOfMeasureId");

			migrationBuilder.CreateIndex(
				name: "IX_lkpRefreshToken_ApplicationUserId",
				table: "lkpRefreshToken",
				column: "ApplicationUserId");

			migrationBuilder.CreateIndex(
				name: "IX_TblAdresses_UserId",
				table: "TblAdresses",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_TblProductCategories_categoryid",
				table: "TblProductCategories",
				column: "categoryid");

			migrationBuilder.CreateIndex(
				name: "IX_TblProductCategories_productid",
				table: "TblProductCategories",
				column: "productid");

			migrationBuilder.CreateIndex(
				name: "IX_TblProductDetails_ProductId",
				table: "TblProductDetails",
				column: "ProductId");

			migrationBuilder.CreateIndex(
				name: "IX_TblProductPhotos_ProductId",
				table: "TblProductPhotos",
				column: "ProductId");

			migrationBuilder.CreateIndex(
				name: "IX_TblProducts_UserId",
				table: "TblProducts",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_TblProductsOrders_OrderId",
				table: "TblProductsOrders",
				column: "OrderId");

			migrationBuilder.CreateIndex(
				name: "IX_TblProductsOrders_ProductId",
				table: "TblProductsOrders",
				column: "ProductId");

			migrationBuilder.CreateIndex(
				name: "IX_TblRestaurantCategories_categoryid",
				table: "TblRestaurantCategories",
				column: "categoryid");

			migrationBuilder.CreateIndex(
				name: "IX_TblRestaurantCategories_restaurantid",
				table: "TblRestaurantCategories",
				column: "restaurantid");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "AspNetRoleClaims");

			migrationBuilder.DropTable(
				name: "AspNetUserClaims");

			migrationBuilder.DropTable(
				name: "AspNetUserLogins");

			migrationBuilder.DropTable(
				name: "AspNetUserRoles");

			migrationBuilder.DropTable(
				name: "AspNetUserTokens");

			migrationBuilder.DropTable(
				name: "LkpCode");

			migrationBuilder.DropTable(
				name: "LkpProductDetailsAttributes");

			migrationBuilder.DropTable(
				name: "lkpRefreshToken");

			migrationBuilder.DropTable(
				name: "LkpUserPhotos");

			migrationBuilder.DropTable(
				name: "TblAdresses");

			migrationBuilder.DropTable(
				name: "TblCustomers");

			migrationBuilder.DropTable(
				name: "TblDrivers");

			migrationBuilder.DropTable(
				name: "TblProductCategories");

			migrationBuilder.DropTable(
				name: "TblProductPhotos");

			migrationBuilder.DropTable(
				name: "TblProductsOrders");

			migrationBuilder.DropTable(
				name: "TblRestaurantCategories");

			migrationBuilder.DropTable(
				name: "AspNetRoles");

			migrationBuilder.DropTable(
				name: "LkpAttributes");

			migrationBuilder.DropTable(
				name: "LkpMeasureUnits");

			migrationBuilder.DropTable(
				name: "TblProductDetails");

			migrationBuilder.DropTable(
				name: "TblCategoryOfProducts");

			migrationBuilder.DropTable(
				name: "TblOrders");

			migrationBuilder.DropTable(
				name: "TblCategoryOfRestaurants");

			migrationBuilder.DropTable(
				name: "TblProducts");

			migrationBuilder.DropTable(
				name: "TblMerchants");

			migrationBuilder.DropTable(
				name: "AspNetUsers");
		}
	}
}
