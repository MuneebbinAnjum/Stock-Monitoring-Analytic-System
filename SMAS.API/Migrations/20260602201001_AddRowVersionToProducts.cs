using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMAS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add row_version column only if it does not already exist (handles runtime-added column)
            migrationBuilder.Sql(@"DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = 'products' AND column_name = 'row_version'
    ) THEN
        ALTER TABLE products ADD COLUMN row_version bytea;
    END IF;
END
$$;");

            migrationBuilder.CreateIndex(
                name: "ix_stock_alerts_product_id_is_resolved",
                table: "stock_alerts",
                columns: new[] { "product_id", "is_resolved" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_stock_alerts_product_id_is_resolved",
                table: "stock_alerts");

            // Drop the column if it exists
            migrationBuilder.Sql("ALTER TABLE products DROP COLUMN IF EXISTS row_version;");
        }
    }
}
