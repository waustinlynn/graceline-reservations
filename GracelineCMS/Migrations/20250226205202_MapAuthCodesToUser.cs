using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GracelineCMS.Migrations
{
    /// <inheritdoc />
    public partial class MapAuthCodesToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailAddress",
                table: "AuthCodes");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "AuthCodes",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthCodes_UserId",
                table: "AuthCodes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuthCodes_Users_UserId",
                table: "AuthCodes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthCodes_Users_UserId",
                table: "AuthCodes");

            migrationBuilder.DropIndex(
                name: "IX_AuthCodes_UserId",
                table: "AuthCodes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AuthCodes");

            migrationBuilder.AddColumn<string>(
                name: "EmailAddress",
                table: "AuthCodes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
