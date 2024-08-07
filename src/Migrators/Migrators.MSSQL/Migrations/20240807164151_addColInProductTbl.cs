using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class addColInProductTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pictures",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "Products",
                newName: "PictureUrl");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Products",
                newName: "SalePrice");

            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PictureName",
                table: "Products",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PictureSize",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PictureName",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PictureSize",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "SalePrice",
                table: "Products",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "PictureUrl",
                table: "Products",
                newName: "Unit");

            migrationBuilder.AddColumn<string>(
                name: "Pictures",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
