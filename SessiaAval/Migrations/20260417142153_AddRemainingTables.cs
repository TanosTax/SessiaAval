using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SessiaAval.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    first_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    balance = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    registration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_users_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "balance_transactions",
                columns: table => new
                {
                    transaction_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    transaction_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    card_last_digits = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_balance_transactions", x => x.transaction_id);
                    table.ForeignKey(
                        name: "FK_balance_transactions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "masters",
                columns: table => new
                {
                    master_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    qualification_level = table.Column<int>(type: "integer", nullable: false),
                    specialization = table.Column<string>(type: "text", nullable: true),
                    hire_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    qualification_request_pending = table.Column<bool>(type: "boolean", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_masters", x => x.master_id);
                    table.ForeignKey(
                        name: "FK_masters_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "appointments",
                columns: table => new
                {
                    appointment_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    master_id = table.Column<int>(type: "integer", nullable: false),
                    service_id = table.Column<int>(type: "integer", nullable: false),
                    appointment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    queue_number = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointments", x => x.appointment_id);
                    table.ForeignKey(
                        name: "FK_appointments_masters_master_id",
                        column: x => x.master_id,
                        principalTable: "masters",
                        principalColumn: "master_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "master_services",
                columns: table => new
                {
                    master_service_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    master_id = table.Column<int>(type: "integer", nullable: false),
                    service_id = table.Column<int>(type: "integer", nullable: false),
                    assigned_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_master_services", x => x.master_service_id);
                    table.ForeignKey(
                        name: "FK_master_services_masters_master_id",
                        column: x => x.master_id,
                        principalTable: "masters",
                        principalColumn: "master_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_master_services_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    review_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    service_id = table.Column<int>(type: "integer", nullable: true),
                    master_id = table.Column<int>(type: "integer", nullable: true),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    review_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.review_id);
                    table.ForeignKey(
                        name: "FK_reviews_masters_master_id",
                        column: x => x.master_id,
                        principalTable: "masters",
                        principalColumn: "master_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reviews_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reviews_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_master_id",
                table: "appointments",
                column: "master_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_service_id",
                table: "appointments",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_user_id",
                table: "appointments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_balance_transactions_user_id",
                table: "balance_transactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_master_services_master_id_service_id",
                table: "master_services",
                columns: new[] { "master_id", "service_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_master_services_service_id",
                table: "master_services",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_masters_user_id",
                table: "masters",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_master_id",
                table: "reviews",
                column: "master_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_service_id",
                table: "reviews",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_user_id",
                table: "reviews",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_role_id",
                table: "users",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appointments");

            migrationBuilder.DropTable(
                name: "balance_transactions");

            migrationBuilder.DropTable(
                name: "master_services");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "masters");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
