using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foodo.Infrastructure.Migrations
{
	/// <inheritdoc />
	public partial class increasedescription : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "ProductDescription",
				table: "TblProducts",
				type: "nvarchar(250)",
				maxLength: 250,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "nvarchar(100)",
				oldMaxLength: 100);

			migrationBuilder.AlterColumn<string>(
				name: "StoreDescription",
				table: "TblMerchants",
				type: "nvarchar(250)",
				maxLength: 250,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "nvarchar(100)",
				oldMaxLength: 100);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "ProductDescription",
				table: "TblProducts",
				type: "nvarchar(100)",
				maxLength: 100,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "nvarchar(250)",
				oldMaxLength: 250);

			migrationBuilder.AlterColumn<string>(
				name: "StoreDescription",
				table: "TblMerchants",
				type: "nvarchar(100)",
				maxLength: 100,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "nvarchar(250)",
				oldMaxLength: 250);
		}
	}
}
