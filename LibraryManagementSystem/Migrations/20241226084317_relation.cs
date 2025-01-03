using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class relation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPayments_UserBooks_UserBookBookId_UserBookUserId",
                table: "UserPayments");

            migrationBuilder.DropIndex(
                name: "IX_UserPayments_UserBookBookId_UserBookUserId",
                table: "UserPayments");

            migrationBuilder.DropColumn(
                name: "UserBookBookId",
                table: "UserPayments");

            migrationBuilder.DropColumn(
                name: "UserBookUserId",
                table: "UserPayments");

            migrationBuilder.CreateIndex(
                name: "IX_UserPayments_UserBookId",
                table: "UserPayments",
                column: "UserBookId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPayments_ReservedBook_UserBookId",
                table: "UserPayments",
                column: "UserBookId",
                principalTable: "ReservedBook",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPayments_ReservedBook_UserBookId",
                table: "UserPayments");

            migrationBuilder.DropIndex(
                name: "IX_UserPayments_UserBookId",
                table: "UserPayments");

            migrationBuilder.AddColumn<int>(
                name: "UserBookBookId",
                table: "UserPayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserBookUserId",
                table: "UserPayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserPayments_UserBookBookId_UserBookUserId",
                table: "UserPayments",
                columns: new[] { "UserBookBookId", "UserBookUserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserPayments_UserBooks_UserBookBookId_UserBookUserId",
                table: "UserPayments",
                columns: new[] { "UserBookBookId", "UserBookUserId" },
                principalTable: "UserBooks",
                principalColumns: new[] { "BookId", "UserId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
