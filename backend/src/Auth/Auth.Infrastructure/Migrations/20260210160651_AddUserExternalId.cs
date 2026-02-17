using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserExternalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ExternalId column with IDENTITY auto-increment
            migrationBuilder.AddColumn<long>(
                name: "ExternalId",
                schema: "auth",
                table: "Users",
                type: "bigint",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            // Create unique index for fast lookups
            migrationBuilder.CreateIndex(
                name: "IX_Users_ExternalId",
                schema: "auth",
                table: "Users",
                column: "ExternalId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop unique index
            migrationBuilder.DropIndex(
                name: "IX_Users_ExternalId",
                schema: "auth",
                table: "Users");

            // Drop ExternalId column
            migrationBuilder.DropColumn(
                name: "ExternalId",
                schema: "auth",
                table: "Users");
        }
    }
}
