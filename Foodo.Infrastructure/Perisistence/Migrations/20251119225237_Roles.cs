using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Foodo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Roles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "23fd934c-7bcf-40e0-a41e-a253a2d3b557", "23fd934c-7bcf-40e0-a41e-a253a2d3b557", "Merchant", "MERCHANT" },
                    { "d6d2f11c-6482-4b9f-8e59-24e997ac635b", "d6d2f11c-6482-4b9f-8e59-24e997ac635b", "Customer", "CUSTOMER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "23fd934c-7bcf-40e0-a41e-a253a2d3b557");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d6d2f11c-6482-4b9f-8e59-24e997ac635b");
        }
    }
}
