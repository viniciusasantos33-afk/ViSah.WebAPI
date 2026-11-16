using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViSah.Migrations
{
    /// <inheritdoc />
    public partial class AddCloudinaryCoverIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverPublicId",
                table: "Tracks",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverPublicId",
                table: "Albums",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverPublicId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "CoverPublicId",
                table: "Albums");
        }
    }
}
