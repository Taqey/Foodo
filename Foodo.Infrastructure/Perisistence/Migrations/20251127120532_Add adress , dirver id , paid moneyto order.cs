using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foodo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addadressdirveridpaidmoneytoorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BillingAddressId",
                table: "TblOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DriverId",
                table: "TblOrders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidMoney",
                table: "TblOrders",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblDrivers");

            migrationBuilder.DropColumn(
                name: "BillingAddressId",
                table: "TblOrders");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "TblOrders");

            migrationBuilder.DropColumn(
                name: "PaidMoney",
                table: "TblOrders");
        }
    }
}
