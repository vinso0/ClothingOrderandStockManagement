using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClothingOrderAndStockManagement.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserIdAndReturnStatusFromReturnLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key constraint if it exists
            migrationBuilder.Sql(@"
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ReturnLogs_AspNetUsers')
            ALTER TABLE ReturnLogs DROP CONSTRAINT FK_ReturnLogs_AspNetUsers
    ");

            // Drop index if it exists
            migrationBuilder.Sql(@"
        IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ReturnLogs_UserId' AND object_id = OBJECT_ID('ReturnLogs'))
            DROP INDEX IX_ReturnLogs_UserId ON ReturnLogs
    ");

            // Drop columns
            migrationBuilder.DropColumn(
                name: "ReturnStatus",
                table: "ReturnLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ReturnLogs");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReturnStatus",
                table: "ReturnLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ReturnLogs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnLogs_UserId",
                table: "ReturnLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnLogs_AspNetUsers",
                table: "ReturnLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
