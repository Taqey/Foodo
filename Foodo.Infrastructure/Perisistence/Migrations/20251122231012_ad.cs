using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foodo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductIOrderId",
                table: "TblProductsOrders",
                newName: "ProductOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductOrderId",
                table: "TblProductsOrders",
                newName: "ProductIOrderId");
        }
    }
}
