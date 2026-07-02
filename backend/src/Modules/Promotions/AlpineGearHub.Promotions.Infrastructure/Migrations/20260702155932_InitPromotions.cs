using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlpineGearHub.Promotions.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitPromotions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "promotions");

            migrationBuilder.CreateTable(
                name: "promotions",
                schema: "promotions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    listing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    start_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    price_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    payment_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    stripe_payment_intent_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_promotions_listing_id",
                schema: "promotions",
                table: "promotions",
                column: "listing_id");

            migrationBuilder.CreateIndex(
                name: "ux_promotions_stripe_payment_intent_id",
                schema: "promotions",
                table: "promotions",
                column: "stripe_payment_intent_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "promotions",
                schema: "promotions");
        }
    }
}
