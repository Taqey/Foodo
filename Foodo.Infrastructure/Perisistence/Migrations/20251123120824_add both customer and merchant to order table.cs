using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foodo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addbothcustomerandmerchanttoordertable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "TblOrders",
                newName: "MerchantId");

            migrationBuilder.AddColumn<string>(
                name: "CustomerId",
                table: "TblOrders",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "TblOrders");

            migrationBuilder.RenameColumn(
                name: "MerchantId",
                table: "TblOrders",
                newName: "UserId");
        }
    }
}
