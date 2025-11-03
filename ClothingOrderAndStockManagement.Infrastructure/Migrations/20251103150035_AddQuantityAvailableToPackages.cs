using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClothingOrderAndStockManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityAvailableToPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuantityAvailable",
                table: "Packages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantityAvailable",
                table: "Packages");
        }
    }
}
