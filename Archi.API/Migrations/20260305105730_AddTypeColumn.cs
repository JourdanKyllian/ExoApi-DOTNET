using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Archi.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Tacos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Pizza",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Tacos");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Pizza");
        }
    }
}
