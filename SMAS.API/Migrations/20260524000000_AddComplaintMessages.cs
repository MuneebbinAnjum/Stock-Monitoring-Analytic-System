using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMAS.API.Migrations
{
    [Microsoft.EntityFrameworkCore.Infrastructure.DbContext(typeof(SMAS.API.Data.SmasDbContext))]
    [Migration("20260524000000_AddComplaintMessages")]
    public partial class AddComplaintMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "complaint_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    complaint_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sender_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    message = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_complaint_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_complaint_messages_complaints_complaint_id",
                        column: x => x.complaint_id,
                        principalTable: "complaints",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_complaint_messages_complaint_id",
                table: "complaint_messages",
                column: "complaint_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "complaint_messages");
        }
    }
}
