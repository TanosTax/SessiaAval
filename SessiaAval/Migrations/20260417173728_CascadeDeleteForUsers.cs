using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessiaAval.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteForUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_appointments_users_user_id",
                table: "appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_balance_transactions_users_user_id",
                table: "balance_transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_masters_users_user_id",
                table: "masters");

            migrationBuilder.DropForeignKey(
                name: "FK_reviews_users_user_id",
                table: "reviews");

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_users_user_id",
                table: "appointments",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_balance_transactions_users_user_id",
                table: "balance_transactions",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_masters_users_user_id",
                table: "masters",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_users_user_id",
                table: "reviews",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_appointments_users_user_id",
                table: "appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_balance_transactions_users_user_id",
                table: "balance_transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_masters_users_user_id",
                table: "masters");

            migrationBuilder.DropForeignKey(
                name: "FK_reviews_users_user_id",
                table: "reviews");

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_users_user_id",
                table: "appointments",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_balance_transactions_users_user_id",
                table: "balance_transactions",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_masters_users_user_id",
                table: "masters",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_users_user_id",
                table: "reviews",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
