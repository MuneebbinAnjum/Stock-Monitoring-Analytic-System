using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMAS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingSchemaObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'employees' AND column_name = 'monthly_salary'
    ) THEN
        ALTER TABLE employees ADD COLUMN monthly_salary numeric(18,2) NOT NULL DEFAULT 0.0;
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_name = 'complaint_messages'
    ) THEN
        IF EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_name = 'complaint_messages' AND column_name = 'sender_type'
        ) THEN
            UPDATE complaint_messages SET sender_type = '' WHERE sender_type IS NULL;
            ALTER TABLE complaint_messages ALTER COLUMN sender_type SET NOT NULL;
        END IF;

        IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_name = 'complaint_messages' AND column_name = 'updated_at'
        ) THEN
            ALTER TABLE complaint_messages ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
        END IF;
    END IF;
END $$;");

            migrationBuilder.CreateTable(
                name: "commissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    commission_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_commissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_commissions_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_commissions_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "discounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discount_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_admin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_discounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_discounts_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_reads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_reads", x => x.id);
                    table.ForeignKey(
                        name: "fk_notification_reads_notifications_notification_id",
                        column: x => x.notification_id,
                        principalTable: "notifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_commissions_employee_id",
                table: "commissions",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_commissions_employee_id_product_id",
                table: "commissions",
                columns: new[] { "employee_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_commissions_product_id",
                table: "commissions",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_discounts_end_date",
                table: "discounts",
                column: "end_date");

            migrationBuilder.CreateIndex(
                name: "ix_discounts_product_id",
                table: "discounts",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_discounts_start_date",
                table: "discounts",
                column: "start_date");

            migrationBuilder.CreateIndex(
                name: "ix_notification_reads_notification_id_user_id_user_type",
                table: "notification_reads",
                columns: new[] { "notification_id", "user_id", "user_type" },
                unique: true);

            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_name = 'complaint_messages'
    ) THEN
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.table_constraints tc
            WHERE tc.table_name = 'complaint_messages'
              AND tc.constraint_type = 'FOREIGN KEY'
              AND tc.constraint_name = 'fk_complaint_messages_complaints_complaint_id'
        ) THEN
            ALTER TABLE complaint_messages
                ADD CONSTRAINT fk_complaint_messages_complaints_complaint_id
                FOREIGN KEY (complaint_id) REFERENCES complaints (id) ON DELETE CASCADE;
        END IF;
    END IF;
END $$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_name = 'complaint_messages'
    ) THEN
        IF EXISTS (
            SELECT 1 FROM information_schema.table_constraints tc
            WHERE tc.table_name = 'complaint_messages'
              AND tc.constraint_type = 'FOREIGN KEY'
              AND tc.constraint_name = 'fk_complaint_messages_complaints_complaint_id'
        ) THEN
            ALTER TABLE complaint_messages
                DROP CONSTRAINT fk_complaint_messages_complaints_complaint_id;
        END IF;

        IF EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_name = 'complaint_messages' AND column_name = 'updated_at'
        ) THEN
            ALTER TABLE complaint_messages DROP COLUMN updated_at;
        END IF;

        IF EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_name = 'complaint_messages' AND column_name = 'sender_type'
        ) THEN
            ALTER TABLE complaint_messages ALTER COLUMN sender_type DROP NOT NULL;
        END IF;
    END IF;
END $$;");

            migrationBuilder.DropTable(
                name: "commissions");

            migrationBuilder.DropTable(
                name: "discounts");

            migrationBuilder.DropTable(
                name: "notification_reads");

            migrationBuilder.DropColumn(
                name: "monthly_salary",
                table: "employees");
        }
    }
}
