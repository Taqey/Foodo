using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foodo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class aa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Tax",
                table: "TblOrders",
                type: "decimal(9,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Tax",
                table: "TblOrders",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,2)",
                oldNullable: true);
        }
    }
}
