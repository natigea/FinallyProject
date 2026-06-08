using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommersProject.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryAvailableToListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DeliveryAvailable",
                table: "Listings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryAvailable",
                table: "Listings");
        }
    }
}
