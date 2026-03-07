using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteSellLaptop.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAccessoryToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAccessory",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAccessory",
                table: "Products");
        }
    }
}
