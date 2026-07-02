using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlpineGearHub.Moderation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "moderation");

            migrationBuilder.CreateTable(
                name: "reports",
                schema: "moderation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    listing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reported_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reviewed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reports", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_reports_listing_id",
                schema: "moderation",
                table: "reports",
                column: "listing_id");

            migrationBuilder.CreateIndex(
                name: "ix_reports_status",
                schema: "moderation",
                table: "reports",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reports",
                schema: "moderation");
        }
    }
}
