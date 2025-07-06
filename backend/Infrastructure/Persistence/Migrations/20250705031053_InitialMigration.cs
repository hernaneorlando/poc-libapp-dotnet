using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    external_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category_entity", x => x.id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "contributors",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    firstname = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    lastname = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    external_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contributor_entity", x => x.id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "publishers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    foundation_date = table.Column<DateTime>(type: "date", nullable: true),
                    phone_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    external_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_publisher_entity", x => x.id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "books",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    isbn = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    edition = table.Column<int>(type: "int", nullable: true),
                    language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    total_pages = table.Column<int>(type: "int", nullable: true),
                    published_date = table.Column<DateTime>(type: "date", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: true),
                    publisher_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    external_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_book_entity", x => x.id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "fk_book_publisher",
                        column: x => x.publisher_id,
                        principalTable: "publishers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_books_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "book_checkouts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    checkout_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    due_date = table.Column<DateTime>(type: "date", nullable: false),
                    return_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    book_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    external_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_book_checkout_entity", x => x.id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "fk_book_checkout_book",
                        column: x => x.book_id,
                        principalTable: "books",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "book_contributors",
                columns: table => new
                {
                    book_id = table.Column<long>(type: "bigint", nullable: false),
                    contributor_id = table.Column<long>(type: "bigint", nullable: false),
                    role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_book_contributors_entity", x => new { x.book_id, x.contributor_id });
                    table.ForeignKey(
                        name: "fk_book_contributors_books_book_id",
                        column: x => x.book_id,
                        principalTable: "books",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_book_contributors_contributors_contributor_id",
                        column: x => x.contributor_id,
                        principalTable: "contributors",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "idx_book_checkout_book_id",
                table: "book_checkouts",
                column: "book_id");

            migrationBuilder.CreateIndex(
                name: "ux_book_checkout_entity_external_id",
                table: "book_checkouts",
                column: "external_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_book_checkout_entity_user_id",
                table: "book_checkouts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_book_contributor_book_id_contributor_id",
                table: "book_contributors",
                columns: new[] { "book_id", "contributor_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_book_contributor_entity_book_id",
                table: "book_contributors",
                column: "book_id");

            migrationBuilder.CreateIndex(
                name: "ix_book_contributor_entity_contributor_id",
                table: "book_contributors",
                column: "contributor_id");

            migrationBuilder.CreateIndex(
                name: "ix_book_entity_category_id",
                table: "books",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_book_publisher_id",
                table: "books",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ux_book_entity_external_id",
                table: "books",
                column: "external_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_book_entity_isbn",
                table: "books",
                column: "isbn",
                unique: true,
                filter: "[isbn] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_category_entity_external_id",
                table: "categories",
                column: "external_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_category_entity_name",
                table: "categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_contributors_firstname_lastname",
                table: "contributors",
                columns: new[] { "firstname", "lastname" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_contributor_entity_external_id",
                table: "contributors",
                column: "external_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_publisher_entity_external_id",
                table: "publishers",
                column: "external_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "book_checkouts");

            migrationBuilder.DropTable(
                name: "book_contributors");

            migrationBuilder.DropTable(
                name: "books");

            migrationBuilder.DropTable(
                name: "contributors");

            migrationBuilder.DropTable(
                name: "publishers");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
