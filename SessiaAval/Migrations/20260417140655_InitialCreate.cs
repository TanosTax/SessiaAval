using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SessiaAval.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "collections",
                columns: table => new
                {
                    collection_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    collection_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collections", x => x.collection_id);
                });

            migrationBuilder.CreateTable(
                name: "service_categories",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_categories", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "services",
                columns: table => new
                {
                    service_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    collection_id = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_services", x => x.service_id);
                    table.ForeignKey(
                        name: "FK_services_collections_collection_id",
                        column: x => x.collection_id,
                        principalTable: "collections",
                        principalColumn: "collection_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_services_service_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "service_categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_services_category_id",
                table: "services",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_services_collection_id",
                table: "services",
                column: "collection_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "services");

            migrationBuilder.DropTable(
                name: "collections");

            migrationBuilder.DropTable(
                name: "service_categories");
        }
    }
}
