using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Foodo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class category : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


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
                        name: "FK_TblRestaurantCategories_TblMerchant_restaurantid",
                        column: x => x.restaurantid,
                        principalTable: "TblMerchant",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_TblProductCategories_categoryid",
                table: "TblProductCategories",
                column: "categoryid");

            migrationBuilder.CreateIndex(
                name: "IX_TblProductCategories_productid",
                table: "TblProductCategories",
                column: "productid");

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
                name: "TblProductCategories");

            migrationBuilder.DropTable(
                name: "TblRestaurantCategories");

            migrationBuilder.DropTable(
                name: "TblCategoryOfProducts");

            migrationBuilder.DropTable(
                name: "TblCategoryOfRestaurants");


        }
    }
}
