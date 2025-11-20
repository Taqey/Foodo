using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foodo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditinglkpRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "lkpRefreshToken");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "lkpRefreshToken",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedOn",
                table: "lkpRefreshToken",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RevokedOn",
                table: "lkpRefreshToken");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "lkpRefreshToken",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                table: "lkpRefreshToken",
                type: "bit",
                nullable: true);
        }
    }
}
