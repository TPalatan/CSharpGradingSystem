using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradingSystem.Migrations
{
    /// <inheritdoc />
    public partial class teacher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePicturePath",
                table: "Teachers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "UserAccountId",
                table: "Teachers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_UserAccountId",
                table: "Teachers",
                column: "UserAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_UserAccounts_UserAccountId",
                table: "Teachers",
                column: "UserAccountId",
                principalTable: "UserAccounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_UserAccounts_UserAccountId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_UserAccountId",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "ProfilePicturePath",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "UserAccountId",
                table: "Teachers");
        }
    }
}
