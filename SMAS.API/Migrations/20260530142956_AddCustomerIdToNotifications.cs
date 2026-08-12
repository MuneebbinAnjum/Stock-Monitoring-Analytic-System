using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMAS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerIdToNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "customer_id",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_customer_id",
                table: "notifications",
                column: "customer_id");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_customers_customer_id",
                table: "notifications",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_notifications_customers_customer_id",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_notifications_customer_id",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "customer_id",
                table: "notifications");
        }
    }
}
