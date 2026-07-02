using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlpineGearHub.Listings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPromotedToListings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_promoted",
                schema: "listings",
                table: "listings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_promoted",
                schema: "listings",
                table: "listings");
        }
    }
}
